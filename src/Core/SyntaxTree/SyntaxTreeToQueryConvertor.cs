using System;
using System.ComponentModel;
using System.Linq.Expressions;

namespace Gridify.Syntax
{
   public static class ExpressionToQueryConvertor
   {
      private static Expression<Func<T, bool>> ConvertBinaryExpressionSyntaxToQuery<T>(BinaryExpressionSyntax binarySyntax, IGridifyQuery gridifyQuery, IGridifyMapper<T> mapper)
      {
         try
         {
            var left = (binarySyntax.Left as FieldExpressionSyntax)?.FieldToken.Text.Trim();
            var right = (binarySyntax.Right as ValueExpressionSyntax)?.ValueToken.Text;
            var op = binarySyntax.OperatorToken;

            if (left == null || right == null) return null;

            var gMap = mapper.GetGMap(left);

            var exp = gMap.To;
            var body = exp.Body;

            // Remove the boxing for value types
            if (body.NodeType == ExpressionType.Convert) body = ((UnaryExpression) body).Operand;

            object value = right;

            // execute user custom Convertor
            if (gMap.Convertor != null)
               value = gMap.Convertor.Invoke(right);


            if (value != null && body.Type != value.GetType())
               try
               {
                  // handle broken guids,  github issue #2
                  if (body.Type == typeof(Guid) && !Guid.TryParse(value.ToString(), out _)) value = Guid.NewGuid();

                  var converter = TypeDescriptor.GetConverter(body.Type);
                  value = converter.ConvertFromString(value.ToString());
               }
               catch (FormatException)
               {
                  // return no records in case of any exception in formating
                  return q => false;
               }

            Expression be = null;
            var containsMethod = typeof(string).GetMethod("Contains", new[] {typeof(string)});
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
                  be = Expression.Call(body, containsMethod, Expression.Constant(value, body.Type));
                  break;
               case SyntaxKind.NotLike:
                  be = Expression.Not(Expression.Call(body, containsMethod, Expression.Constant(value, body.Type)));
                  break;
               default:
                  return null;
            }

            return Expression.Lambda<Func<T, bool>>(be, exp.Parameters);
         }
         catch (Exception)
         {
            return null;
         }
      }

      internal static Expression<Func<T, bool>> GenerateQuery<T>(ExpressionSyntax expression, IGridifyQuery gridifyQuery, IGridifyMapper<T> mapper)
      {
         switch (expression.Kind)
         {
            case SyntaxKind.BinaryExpression:
            {
               var bExp = expression as BinaryExpressionSyntax;

               if (bExp!.Left is FieldExpressionSyntax && bExp.Right is ValueExpressionSyntax)
                  return ConvertBinaryExpressionSyntaxToQuery(bExp, gridifyQuery, mapper);

               Expression<Func<T, bool>> leftQuery;
               Expression<Func<T, bool>> rightQuery;


               if (bExp.Left is ParenthesizedExpressionSyntax lpExp)
                  leftQuery = GenerateQuery(lpExp.Expression, gridifyQuery, mapper);
               else
                  leftQuery = GenerateQuery(bExp.Left, gridifyQuery, mapper);


               if (bExp.Right is ParenthesizedExpressionSyntax rpExp)
                  rightQuery = GenerateQuery(rpExp.Expression, gridifyQuery, mapper);
               else
                  rightQuery = GenerateQuery(bExp.Right, gridifyQuery, mapper);


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
               return GenerateQuery(pExp!.Expression, gridifyQuery, mapper);
            }
            default:
               throw new GridifyFilteringException($"Invalid expression format '{expression.Kind}'.");
         }
      }
   }
}