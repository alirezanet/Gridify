using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Gridify.Syntax
{
   public static class ExpressionToQueryConvertor
   {
      private static Expression<Func<T, bool>>? ConvertBinaryExpressionSyntaxToQuery<T>(BinaryExpressionSyntax binarySyntax, IGridifyMapper<T> mapper)
      {
            var left = (binarySyntax.Left as FieldExpressionSyntax)?.FieldToken.Text.Trim();
            var right = (binarySyntax.Right as ValueExpressionSyntax)?.ValueToken.Text;
            var op = binarySyntax.OperatorToken;

            if (left == null || right == null) return null;

            var gMap = mapper.GetGMap(left);

            if (gMap == null) return null;

            if (gMap.IsNestedCollection)
               return GenerateNestedExpression(mapper, gMap, right, op);

            return GenerateExpression(gMap.To.Body, gMap.To.Parameters[0], right,
               op, mapper.Configuration.AllowNullSearch, gMap.Convertor) as Expression<Func<T, bool>>;
      }

      private static Expression<Func<T, bool>>? GenerateNestedExpression<T>(
         IGridifyMapper<T> mapper,
         IGMap<T> gMap,
         string stringValue,
         SyntaxNode op)
      {
         var body = gMap.To.Body;

         if (body is MethodCallExpression selectExp && selectExp.Method.Name == "Select")
         {
            var targetExp = selectExp.Arguments.Single(a => a.NodeType == ExpressionType.Lambda) as LambdaExpression;
            var conditionExp = GenerateExpression(targetExp!.Body, targetExp.Parameters[0], stringValue, op, mapper.Configuration.AllowNullSearch,
               gMap.Convertor);

            if (conditionExp == null) return null;

            return ParseMethodCallExpression(selectExp, conditionExp) as Expression<Func<T, bool>>;
         }

         // this should never happening
         throw new GridifyFilteringException($"The 'Select' method on '{gMap.From}' not found");
      }

      private static LambdaExpression ParseMethodCallExpression(MethodCallExpression exp, LambdaExpression predicate)
      {
         switch (exp.Arguments.First())
         {
            case MemberExpression member:
               return GetAnyExpression(member, predicate);
            case MethodCallExpression subExp when subExp.Method.Name == "SelectMany" &&  subExp.Arguments.Last() is LambdaExpression { Body: MemberExpression lambdaMember }:
            {
               var newPredicate = GetAnyExpression(lambdaMember, predicate);
               return ParseMethodCallExpression(subExp, newPredicate);
            }
            case MethodCallExpression subExp when subExp.Method.Name == "Select" && subExp.Arguments.Last() is LambdaExpression { Body: MemberExpression lambdaMember } lambda:
            {
               var newExp = new PredicateBuilder.ReplaceExpressionVisitor(predicate.Parameters[0], lambdaMember).Visit(predicate.Body);
               var newPredicate = GetExpressionWithNullCheck(lambdaMember, lambda.Parameters[0], newExp!); 
               return ParseMethodCallExpression(subExp, newPredicate);
            }
            default:
               throw new InvalidOperationException();
         }
      }

      private static ParameterExpression GetParameterExpression(MemberExpression member)
      {
         return Expression.Parameter(member.Expression.Type, member.Expression.ToString()); 
      }
      private static LambdaExpression GetAnyExpression(MemberExpression member, Expression predicate)
      {
         var param = GetParameterExpression(member);
         var prop = Expression.Property(param!, member.Member.Name);

         var tp = prop.Type.GenericTypeArguments[0];
         var anyMethod = GetAnyMethod(tp);
         var anyExp = Expression.Call(anyMethod, prop, predicate);

         return GetExpressionWithNullCheck(prop, param, anyExp);
         
         // return Expression.Lambda(anyExp, param);
      }

      private static LambdaExpression GetExpressionWithNullCheck(MemberExpression prop, ParameterExpression param, Expression right)
      {
         var nullChecker = Expression.NotEqual(prop, Expression.Constant(null));
         var exp = Expression.AndAlso(nullChecker, right);
         return Expression.Lambda(exp, param); 
      }

      private static LambdaExpression? GenerateExpression(
         Expression body,
         ParameterExpression parameter,
         string stringValue,
         SyntaxNode op,
         bool allowNullSearch,
         Func<string, object>? convertor)
      {
         // Remove the boxing for value types
         if (body.NodeType == ExpressionType.Convert) body = ((UnaryExpression)body).Operand;

         object? value = stringValue;

         // execute user custom Convertor
         if (convertor != null)
            value = convertor.Invoke(stringValue);

         if (value != null && body.Type != value.GetType())
            try
            {
               if (allowNullSearch && op.Kind is SyntaxKind.Equal or SyntaxKind.NotEqual && value.ToString() == "null")
                  value = null;
               else
               {
                  // handle broken guids,  github issue #2
                  if (body.Type == typeof(Guid) && !Guid.TryParse(value.ToString(), out _)) value = Guid.NewGuid().ToString();

                  var converter = TypeDescriptor.GetConverter(body.Type);
                  value = converter.ConvertFromString(value.ToString())!;
               }
            }
            catch (FormatException)
            {
               // return no records in case of any exception in formatting
               return Expression.Lambda(Expression.Constant(false), parameter); // q => false
            }

         Expression be;

         switch (op.Kind)
         {
            case SyntaxKind.Equal:
               be = Expression.Equal(body, Expression.Constant(value, body.Type));
               break;
            case SyntaxKind.NotEqual:
               be = Expression.NotEqual(body, Expression.Constant(value, body.Type));
               break;
            case SyntaxKind.GreaterThan:
               be = Expression.GreaterThan(body, Expression.Constant(value, body.Type));
               break;
            case SyntaxKind.LessThan:
               be = Expression.LessThan(body, Expression.Constant(value, body.Type));
               break;
            case SyntaxKind.GreaterOrEqualThan:
               be = Expression.GreaterThanOrEqual(body, Expression.Constant(value, body.Type));
               break;
            case SyntaxKind.LessOrEqualThan:
               be = Expression.LessThanOrEqual(body, Expression.Constant(value, body.Type));
               break;
            case SyntaxKind.Like:
               be = Expression.Call(body, GetContainsMethod(), Expression.Constant(value, body.Type));
               break;
            case SyntaxKind.NotLike:
               be = Expression.Not(Expression.Call(body, GetContainsMethod(), Expression.Constant(value, body.Type)));
               break;
            case SyntaxKind.StartsWith:
               if (body.Type != typeof(string))
               {
                  body = Expression.Call(body, GetToStringMethod());
                  be = Expression.Call(body, GetStartWithMethod(), Expression.Constant(value?.ToString(), body.Type));
               }
               else
                  be = Expression.Call(body, GetStartWithMethod(), Expression.Constant(value, body.Type));

               break;
            case SyntaxKind.EndsWith:
               if (body.Type != typeof(string))
               {
                  body = Expression.Call(body, GetToStringMethod());
                  be = Expression.Call(body, GetEndsWithMethod(), Expression.Constant(value?.ToString(), body.Type));
               }
               else
                  be = Expression.Call(body, GetEndsWithMethod(), Expression.Constant(value, body.Type));

               break;
            case SyntaxKind.NotStartsWith:
               if (body.Type != typeof(string))
               {
                  body = Expression.Call(body, GetToStringMethod());
                  be = Expression.Not(Expression.Call(body, GetStartWithMethod(), Expression.Constant(value?.ToString(), body.Type)));
               }
               else
                  be = Expression.Not(Expression.Call(body, GetStartWithMethod(), Expression.Constant(value, body.Type)));

               break;
            case SyntaxKind.NotEndsWith:
               if (body.Type != typeof(string))
               {
                  body = Expression.Call(body, GetToStringMethod());
                  be = Expression.Not(Expression.Call(body, GetEndsWithMethod(), Expression.Constant(value?.ToString(), body.Type)));
               }
               else
                  be = Expression.Not(Expression.Call(body, GetEndsWithMethod(), Expression.Constant(value, body.Type)));

               break;
            default:
               return null;
         }

         return Expression.Lambda(be, parameter);
      }

      private static MethodInfo GetAnyMethod(Type @type) =>
         typeof(Enumerable).GetMethods().Single(m => m.Name == "Any" && m.GetParameters().Length == 2).MakeGenericMethod(@type);

      private static MethodInfo GetEndsWithMethod() => typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!;

      private static MethodInfo GetStartWithMethod() => typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!;

      private static MethodInfo GetContainsMethod() => typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

      private static MethodInfo GetToStringMethod() => typeof(object).GetMethod("ToString")!;

      internal static Expression<Func<T, bool>> GenerateQuery<T>(ExpressionSyntax expression, IGridifyMapper<T> mapper)
      {
         while (true)
            switch (expression.Kind)
            {
               case SyntaxKind.BinaryExpression:
               {
                  var bExp = expression as BinaryExpressionSyntax;

                  if (bExp!.Left is FieldExpressionSyntax && bExp.Right is ValueExpressionSyntax)
                     return ConvertBinaryExpressionSyntaxToQuery(bExp, mapper) ?? throw new GridifyFilteringException("Invalid expression");

                  Expression<Func<T, bool>> leftQuery;
                  Expression<Func<T, bool>> rightQuery;


                  if (bExp.Left is ParenthesizedExpressionSyntax lpExp)
                     leftQuery = GenerateQuery(lpExp.Expression, mapper);
                  else
                     leftQuery = GenerateQuery(bExp.Left, mapper);


                  if (bExp.Right is ParenthesizedExpressionSyntax rpExp)
                     rightQuery = GenerateQuery(rpExp.Expression, mapper);
                  else
                     rightQuery = GenerateQuery(bExp.Right, mapper);


                  return bExp.OperatorToken.Kind switch
                  {
                     SyntaxKind.And => leftQuery.And(rightQuery),
                     SyntaxKind.Or => leftQuery.Or(rightQuery),
                     _ => throw new GridifyFilteringException($"Invalid expression Operator '{bExp.OperatorToken.Kind}'")
                  };
               }
               case SyntaxKind.ParenthesizedExpression:
               {
                  var pExp = expression as ParenthesizedExpressionSyntax;
                  expression = pExp!.Expression;
                  continue;
               }
               default:
                  throw new GridifyFilteringException($"Invalid expression format '{expression.Kind}'.");
            }
      }
   }
}