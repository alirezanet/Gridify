using System;
using System.Linq.Expressions;
using System.Text;

namespace Gridify.Elasticsearch;

internal static class ExpressionExtensions
{
   internal static string ToPropertyPath(this Expression expression)
   {
      var memberAccessList = new StringBuilder();
      VisitMemberAccessChain(expression, memberAccessList);

      return memberAccessList.ToString();
   }

   public static Type GetRealType<T>(this Expression<Func<T, object>> expression)
   {
      if (expression.Body is MemberExpression memberExpression)
         return memberExpression.Type;

      if (expression.Body is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
         return unaryExpression.Operand.Type;

      throw new InvalidOperationException("Unsupported expression type.");
   }

   private static void VisitMemberAccessChain(Expression expression, StringBuilder result)
   {
      if (expression is MemberExpression memberExpression)
      {
         if (result.Length > 0)
         {
            result.Insert(0, ".");
         }

         result.Insert(0, memberExpression.Member.Name);

         VisitMemberAccessChain(memberExpression.Expression, result);
      }
      else if (expression is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
      {
         // Handle cases when TValue is object and implicit conversion exists
         VisitMemberAccessChain(unaryExpression.Operand, result);
      }
   }
}
