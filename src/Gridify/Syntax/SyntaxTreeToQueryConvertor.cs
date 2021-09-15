using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;

namespace Gridify.Syntax
{
   public static class ExpressionToQueryConvertor
   {
      private static Expression<Func<T, bool>>? ConvertBinaryExpressionSyntaxToQuery<T>(BinaryExpressionSyntax binarySyntax, IGridifyMapper<T> mapper)
      {
         try
         {
            var left = (binarySyntax.Left as FieldExpressionSyntax)?.FieldToken.Text.Trim();
            var right = (binarySyntax.Right as ValueExpressionSyntax)?.ValueToken.Text;
            var op = binarySyntax.OperatorToken;

            if (left == null || right == null) return null;

            var gMap = mapper.GetGMap(left);
            if (gMap == null) return null;

            if (!gMap.IsNestedCollection)
               return GenerateExpression(mapper, gMap, right, op);
            
            return GenerateNestedExpression(mapper, gMap, right, op);

         }
         catch (Exception)
         {
            // Unhandled exceptions ignores gridify completely,
            // Not sure this is the best approach or not yet
            return null;
         }
      }

      private static Expression<Func<T, bool>> GenerateNestedExpression<T>(IGridifyMapper<T> mapper, IGMap<T> gMap, string right, SyntaxToken op)
      {
         throw new NotImplementedException();
      }

      private static Expression<Func<T, bool>>? GenerateExpression<T>(IGridifyMapper<T> mapper, IGMap<T> gMap, string stringValue, SyntaxToken op)
      {
         var exp = gMap.To;
         var body = exp.Body;

         // Remove the boxing for value types
         if (body.NodeType == ExpressionType.Convert) body = ((UnaryExpression) body).Operand;

         object? value = stringValue;

         // execute user custom Convertor
         if (gMap.Convertor != null)
            value = gMap.Convertor.Invoke(stringValue);

         if (value != null && body.Type != value.GetType())
            try
            {
               if (mapper.Configuration.AllowNullSearch && op.Kind is SyntaxKind.Equal or SyntaxKind.NotEqual && value.ToString() == "null")
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
               return q => false;
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

         return Expression.Lambda<Func<T, bool>>(be, exp.Parameters);
      }

      private static MethodInfo GetEndsWithMethod() => typeof(string).GetMethod("EndsWith", new[] {typeof(string)})!;

      private static MethodInfo GetStartWithMethod() => typeof(string).GetMethod("StartsWith", new[] {typeof(string)})!;

      private static MethodInfo GetContainsMethod() => typeof(string).GetMethod("Contains", new[] {typeof(string)})!;

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