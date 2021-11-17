using System;
using System.ComponentModel;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Gridify.Syntax
{
   public static class ExpressionToQueryConvertor
   {
      private static (Expression<Func<T, bool>> Expression, bool IsNested)? ConvertBinaryExpressionSyntaxToQuery<T>(
         BinaryExpressionSyntax binarySyntax, IGridifyMapper<T> mapper)
      {
         var fieldExpression = binarySyntax.Left as FieldExpressionSyntax;

         var left = fieldExpression?.FieldToken.Text.Trim();
         var right = (binarySyntax.Right as ValueExpressionSyntax);
         var op = binarySyntax.OperatorToken;

         if (left == null || right == null) return null;

         var gMap = mapper.GetGMap(left);

         if (gMap == null) throw new GridifyMapperException($"Mapping '{left}' not found");

         if (fieldExpression!.IsCollection)
            gMap.To = UpdateExpressionIndex(gMap.To, fieldExpression.Index);

         var isNested = ((GMap<T>)gMap).IsNestedCollection();
         if (isNested)
         {
            var result = GenerateNestedExpression(mapper, gMap, right, op);
            if (result == null) return null;
            return (result, isNested);
         }
         else
         {
            if (GenerateExpression(gMap.To.Body, gMap.To.Parameters[0], right,
               op, mapper.Configuration.AllowNullSearch, gMap.Convertor) is not Expression<Func<T, bool>> result) return null;
            return (result, false);
         }
      }

      private static LambdaExpression UpdateExpressionIndex(LambdaExpression exp, int index)
      {
         var body = new PredicateBuilder.ReplaceExpressionVisitor(exp.Parameters[1], Expression.Constant(index, typeof(int))).Visit(exp.Body);
         return Expression.Lambda(body, exp.Parameters);
      }

      private static Expression<Func<T, bool>>? GenerateNestedExpression<T>(
         IGridifyMapper<T> mapper,
         IGMap<T> gMap,
         ValueExpressionSyntax value,
         SyntaxNode op)
      {
         var body = gMap.To.Body;

         if (body is MethodCallExpression selectExp && selectExp.Method.Name == "Select")
         {
            var targetExp = selectExp.Arguments.Single(a => a.NodeType == ExpressionType.Lambda) as LambdaExpression;
            var conditionExp = GenerateExpression(targetExp!.Body, targetExp.Parameters[0], value, op, mapper.Configuration.AllowNullSearch,
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
            case MethodCallExpression subExp when subExp.Method.Name == "SelectMany" &&
                                                  subExp.Arguments.Last() is LambdaExpression { Body: MemberExpression lambdaMember }:
            {
               var newPredicate = GetAnyExpression(lambdaMember, predicate);
               return ParseMethodCallExpression(subExp, newPredicate);
            }
            case MethodCallExpression subExp when subExp.Method.Name == "Select" && subExp.Arguments.Last() is LambdaExpression
            {
               Body: MemberExpression lambdaMember
            } lambda:
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
         var prop = Expression.PropertyOrField(param!, member.Member.Name);

         var tp = prop.Type.GenericTypeArguments[0];
         var anyMethod = GetAnyMethod(tp);
         var anyExp = Expression.Call(anyMethod, prop, predicate);

         return GetExpressionWithNullCheck(prop, param, anyExp);
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
         ValueExpressionSyntax valueExpression,
         SyntaxNode op,
         bool allowNullSearch,
         Func<string, object>? convertor)
      {
         // Remove the boxing for value types
         if (body.NodeType == ExpressionType.Convert) body = ((UnaryExpression)body).Operand;

         object? value = valueExpression.ValueToken.Text;

         // execute user custom Convertor
         if (convertor != null)
            value = convertor.Invoke(valueExpression.ValueToken.Text) ?? null;

         // handle the `null` keyword in value
         if (allowNullSearch && op.Kind is SyntaxKind.Equal or SyntaxKind.NotEqual && value?.ToString() == "null")
            value = null;

         // type fixer
         if (value is not null && body.Type != value.GetType())
         {
            try
            {
               // handle broken guids,  github issue #2
               if (body.Type == typeof(Guid) && !Guid.TryParse(value!.ToString(), out _)) value = Guid.NewGuid().ToString();

               var converter = TypeDescriptor.GetConverter(body.Type);
               value = converter.ConvertFromString(value!.ToString())!;
            }
            catch (FormatException)
            {
               // return no records in case of any exception in formatting
               return Expression.Lambda(Expression.Constant(false), parameter); // q => false
            }
         }

         // handle case-Insensitive search 
         if (value is not null && valueExpression.IsCaseInsensitive
                               && op.Kind is not SyntaxKind.GreaterThan
                               && op.Kind is not SyntaxKind.LessThan
                               && op.Kind is not SyntaxKind.GreaterOrEqualThan
                               && op.Kind is not SyntaxKind.LessOrEqualThan)
         {
            value = value.ToString().ToLower();
            body = Expression.Call(body, GetToLowerMethod());
         }

         Expression be;

         // use string.Compare instead of operators if value and field are both strings
         var areBothStrings = body.Type == typeof(string) && value?.GetType() == typeof(string);

         switch (op.Kind)
         {
            case SyntaxKind.Equal when !valueExpression.IsNullOrDefault:
               be = Expression.Equal(body, GetValueExpression(body.Type, value));
               break;
            case SyntaxKind.Equal when valueExpression.IsNullOrDefault:
               if (body.Type == typeof(string))
                  be = Expression.Call(null, GetIsNullOrEmptyMethod(), body);
               else
               {
                  var canBeNull = !body.Type.IsValueType || (Nullable.GetUnderlyingType(body.Type) != null);
                  be = canBeNull
                     ? Expression.OrElse(Expression.Equal(body, Expression.Constant(null)), Expression.Equal(body, Expression.Default(body.Type)))
                     : Expression.Equal(body, Expression.Default(body.Type));
               }

               break;
            case SyntaxKind.NotEqual when !valueExpression.IsNullOrDefault:
               be = Expression.NotEqual(body, GetValueExpression(body.Type, value));
               break;
            case SyntaxKind.NotEqual when valueExpression.IsNullOrDefault:
               if (body.Type == typeof(string))
                  be = Expression.Not(Expression.Call(null, GetIsNullOrEmptyMethod(), body));
               else
               {
                  var canBeNull = !body.Type.IsValueType || (Nullable.GetUnderlyingType(body.Type) != null);
                  be = canBeNull
                     ? Expression.AndAlso(Expression.NotEqual(body, Expression.Constant(null)),
                        Expression.NotEqual(body, Expression.Default(body.Type)))
                     : Expression.NotEqual(body, Expression.Default(body.Type));
               }

               break;
            case SyntaxKind.GreaterThan when areBothStrings == false:
               be = Expression.GreaterThan(body, GetValueExpression(body.Type, value));
               break;
            case SyntaxKind.LessThan when areBothStrings == false:
               be = Expression.LessThan(body, GetValueExpression(body.Type, value));
               break;
            case SyntaxKind.GreaterOrEqualThan when areBothStrings == false:
               be = Expression.GreaterThanOrEqual(body, GetValueExpression(body.Type, value));
               break;
            case SyntaxKind.LessOrEqualThan when areBothStrings == false:
               be = Expression.LessThanOrEqual(body, GetValueExpression(body.Type, value));
               break;
            case SyntaxKind.GreaterThan when areBothStrings:
               be = GetGreaterThanExpression(body, valueExpression, value);
               break;
            case SyntaxKind.LessThan when areBothStrings:
               be = GetLessThanExpression(body, valueExpression, value);
               break;
            case SyntaxKind.GreaterOrEqualThan when areBothStrings:
               be = GetGreaterThanOrEqualExpression(body, valueExpression, value);
               break;
            case SyntaxKind.LessOrEqualThan when areBothStrings:
               be = GetLessThanOrEqualExpression(body, valueExpression, value);
               break;
            case SyntaxKind.Like:
               be = Expression.Call(body, GetContainsMethod(), GetValueExpression(body.Type, value));
               break;
            case SyntaxKind.NotLike:
               be = Expression.Not(Expression.Call(body, GetContainsMethod(), GetValueExpression(body.Type, value)));
               break;
            case SyntaxKind.StartsWith:
               if (body.Type != typeof(string))
               {
                  body = Expression.Call(body, GetToStringMethod());
                  be = Expression.Call(body, GetStartWithMethod(), GetValueExpression(body.Type, value?.ToString()));
               }
               else
                  be = Expression.Call(body, GetStartWithMethod(), GetValueExpression(body.Type, value));

               break;
            case SyntaxKind.EndsWith:
               if (body.Type != typeof(string))
               {
                  body = Expression.Call(body, GetToStringMethod());
                  be = Expression.Call(body, GetEndsWithMethod(), GetValueExpression(body.Type, value?.ToString()));
               }
               else
                  be = Expression.Call(body, GetEndsWithMethod(), GetValueExpression(body.Type, value));

               break;
            case SyntaxKind.NotStartsWith:
               if (body.Type != typeof(string))
               {
                  body = Expression.Call(body, GetToStringMethod());
                  be = Expression.Not(Expression.Call(body, GetStartWithMethod(), GetValueExpression(body.Type, value?.ToString())));
               }
               else
                  be = Expression.Not(Expression.Call(body, GetStartWithMethod(), GetValueExpression(body.Type, value)));

               break;
            case SyntaxKind.NotEndsWith:
               if (body.Type != typeof(string))
               {
                  body = Expression.Call(body, GetToStringMethod());
                  be = Expression.Not(Expression.Call(body, GetEndsWithMethod(), GetValueExpression(body.Type, value?.ToString())));
               }
               else
                  be = Expression.Not(Expression.Call(body, GetEndsWithMethod(), GetValueExpression(body.Type, value)));

               break;
            default:
               return null;
         }

         return Expression.Lambda(be, parameter);
      }

      private static Expression GetValueExpression(Type type, object? value)
      {
         if (!GridifyExtensions.EntityFrameworkCompatibilityLayer)
            return Expression.Constant(value, type);

         // active parameterized query for EF 
         const string fieldName = "Value";
         var (instance, type1) = GridifyTypeBuilder.CreateNewObject(type, fieldName, value);
         return Expression.PropertyOrField(Expression.Constant(instance, type1), fieldName);
      }

      private static BinaryExpression GetLessThanOrEqualExpression(Expression body, ValueExpressionSyntax valueExpression, object? value)
      {
         if (GridifyExtensions.EntityFrameworkCompatibilityLayer)
            return Expression.LessThanOrEqual(Expression.Call(null, GetCompareMethod(), body, GetValueExpression(typeof(string), value)),
               Expression.Constant(0));

         return Expression.LessThanOrEqual(Expression.Call(null, GetCompareMethodWithStringComparison(), body,
            GetValueExpression(typeof(string), value),
            GetStringComparisonCaseExpression(valueExpression.IsCaseInsensitive)), Expression.Constant(0));
      }

      private static BinaryExpression GetGreaterThanOrEqualExpression(Expression body, ValueExpressionSyntax valueExpression, object? value)
      {
         if (GridifyExtensions.EntityFrameworkCompatibilityLayer)
            return Expression.GreaterThanOrEqual(Expression.Call(null, GetCompareMethod(), body, GetValueExpression(typeof(string), value)),
               Expression.Constant(0));

         return Expression.GreaterThanOrEqual(Expression.Call(null, GetCompareMethodWithStringComparison(), body,
            GetValueExpression(typeof(string), value),
            GetStringComparisonCaseExpression(valueExpression.IsCaseInsensitive)), Expression.Constant(0));
      }

      private static BinaryExpression GetLessThanExpression(Expression body, ValueExpressionSyntax valueExpression, object? value)
      {
         if (GridifyExtensions.EntityFrameworkCompatibilityLayer)
            return Expression.LessThan(Expression.Call(null, GetCompareMethod(), body, GetValueExpression(typeof(string), value)),
               Expression.Constant(0));

         return Expression.LessThan(Expression.Call(null, GetCompareMethodWithStringComparison(), body,
            GetValueExpression(typeof(string), value),
            GetStringComparisonCaseExpression(valueExpression.IsCaseInsensitive)), Expression.Constant(0));
      }

      private static BinaryExpression GetGreaterThanExpression(Expression body, ValueExpressionSyntax valueExpression, object? value)
      {
         if (GridifyExtensions.EntityFrameworkCompatibilityLayer)
            return Expression.GreaterThan(Expression.Call(null, GetCompareMethod(), body, GetValueExpression(typeof(string), value)),
               Expression.Constant(0));

         return Expression.GreaterThan(Expression.Call(null, GetCompareMethodWithStringComparison(), body,
            GetValueExpression(typeof(string), value),
            GetStringComparisonCaseExpression(valueExpression.IsCaseInsensitive)), Expression.Constant(0));
      }

      private static ConstantExpression GetStringComparisonCaseExpression(bool isCaseInsensitive)
      {
         return isCaseInsensitive
            ? Expression.Constant(StringComparison.OrdinalIgnoreCase)
            : Expression.Constant(StringComparison.Ordinal);
      }

      private static MethodInfo GetAnyMethod(Type @type) =>
         typeof(Enumerable).GetMethods().Single(m => m.Name == "Any" && m.GetParameters().Length == 2).MakeGenericMethod(@type);

      private static MethodInfo GetEndsWithMethod() => typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!;

      private static MethodInfo GetStartWithMethod() => typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!;

      private static MethodInfo GetContainsMethod() => typeof(string).GetMethod("Contains", new[] { typeof(string) })!;
      private static MethodInfo GetIsNullOrEmptyMethod() => typeof(string).GetMethod("IsNullOrEmpty", new[] { typeof(string) })!;
      private static MethodInfo GetToLowerMethod() => typeof(string).GetMethod("ToLower", new Type[] { })!;

      private static MethodInfo GetCompareMethodWithStringComparison() =>
         typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string), typeof(StringComparison) })!;

      private static MethodInfo GetCompareMethod() =>
         typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) })!;

      private static MethodInfo GetToStringMethod() => typeof(object).GetMethod("ToString")!;


      internal static (Expression<Func<T, bool>> Expression, bool IsNested)
         GenerateQuery<T>(ExpressionSyntax expression, IGridifyMapper<T> mapper, bool isParenthesisOpen = false)
      {
         while (true)
            switch (expression.Kind)
            {
               case SyntaxKind.BinaryExpression:
               {
                  var bExp = expression as BinaryExpressionSyntax;

                  if (bExp!.Left is FieldExpressionSyntax && bExp.Right is ValueExpressionSyntax)
                  {
                     try
                     {
                        return ConvertBinaryExpressionSyntaxToQuery(bExp, mapper) ?? throw new GridifyFilteringException("Invalid expression");
                     }
                     catch (GridifyMapperException e)
                     {
                        if (mapper.Configuration.IgnoreNotMappedFields)
                           return (_ => true, false);

                        throw;
                     }
                  }

                  (Expression<Func<T, bool>> exp, bool isNested) leftQuery;
                  (Expression<Func<T, bool>> exp, bool isNested) rightQuery;

                  if (bExp.Left is ParenthesizedExpressionSyntax lpExp)
                  {
                     leftQuery = GenerateQuery(lpExp.Expression, mapper, true);
                  }
                  else
                     leftQuery = GenerateQuery(bExp.Left, mapper);


                  if (bExp.Right is ParenthesizedExpressionSyntax rpExp)
                     rightQuery = GenerateQuery(rpExp.Expression, mapper, true);
                  else
                     rightQuery = GenerateQuery(bExp.Right, mapper);

                  // check for nested collections
                  if (isParenthesisOpen &&
                      CheckIfCanMerge(leftQuery, rightQuery, bExp.OperatorToken.Kind) is Expression<Func<T, bool>> mergedResult)
                     return (mergedResult, true);

                  var result = bExp.OperatorToken.Kind switch
                  {
                     SyntaxKind.And => leftQuery.exp.And(rightQuery.exp),
                     SyntaxKind.Or => leftQuery.exp.Or(rightQuery.exp),
                     _ => throw new GridifyFilteringException($"Invalid expression Operator '{bExp.OperatorToken.Kind}'")
                  };
                  return (result, false);
               }
               case SyntaxKind.ParenthesizedExpression: // first entrypoint only
               {
                  var pExp = expression as ParenthesizedExpressionSyntax;
                  return GenerateQuery(pExp!.Expression, mapper, true);
               }
               default:
                  throw new GridifyFilteringException($"Invalid expression format '{expression.Kind}'.");
            }
      }

      private static LambdaExpression? CheckIfCanMerge<T>((Expression<Func<T, bool>> exp, bool isNested) leftQuery,
         (Expression<Func<T, bool>> exp, bool isNested) rightQuery, SyntaxKind op)
      {
         if (leftQuery.isNested && rightQuery.isNested)
         {
            var leftExp = ParseNestedExpression(leftQuery.exp.Body);
            var rightExp = ParseNestedExpression(rightQuery.exp.Body);

            if (leftExp.Arguments.First() is MemberExpression leftMember &&
                rightExp.Arguments.First() is MemberExpression rightMember &&
                leftMember.Type == rightMember.Type)
            {
               // we can merge 
               var leftLambda = leftExp.Arguments.Last() as LambdaExpression;
               var rightLambda = rightExp.Arguments.Last() as LambdaExpression;

               if (leftLambda is null || rightLambda is null)
                  return null;

               var visitedRight = new PredicateBuilder.ReplaceExpressionVisitor(rightLambda.Parameters[0], leftLambda.Parameters[0])
                  .Visit(rightLambda.Body);

               var mergedExpression = op switch
               {
                  SyntaxKind.And => Expression.AndAlso(leftLambda.Body, visitedRight),
                  SyntaxKind.Or => Expression.OrElse(leftLambda.Body, visitedRight),
                  _ => throw new InvalidOperationException()
               };

               var mergedLambda = Expression.Lambda(mergedExpression, leftLambda.Parameters);
               var newLambda = GetAnyExpression(leftMember, mergedLambda) as Expression<Func<T, bool>>;
               return newLambda;
            }
         }

         return null;
      }

      private static MethodCallExpression ParseNestedExpression(Expression exp)
      {
         return exp switch
         {
            BinaryExpression { Right: MethodCallExpression cExp } => cExp,
            MethodCallExpression mcExp => mcExp,
            _ => throw new InvalidExpressionException()
         };
      }
   }
}