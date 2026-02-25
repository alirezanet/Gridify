using System;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;

namespace Gridify.Elasticsearch;

internal static class ExpressionExtensions
{
   internal static Type GetRealType<T>(this Expression<Func<T, object>> expression)
   {
      if (expression.Body is MemberExpression memberExpression)
         return memberExpression.Type;

      if (expression.Body is UnaryExpression { NodeType: ExpressionType.Convert } unaryExpression)
         return unaryExpression.Operand.Type;

      throw new InvalidOperationException("Unsupported expression type.");
   }

   internal static string BuildFieldName<T>(this Expression expression, bool isStringValue, IGridifyMapper<T> mapper)
   {
      var propertyPath = expression.ToPropertyPath();
      var propertyPathParts = propertyPath.Split('.');
      propertyPath = string.Join(".", propertyPathParts.Select(
         mapper.Configuration.CustomElasticsearchNamingAction ?? JsonNamingPolicy.CamelCase.ConvertName));

      return isStringValue ? $"{propertyPath}.keyword" : propertyPath;
   }

   private static string ToPropertyPath(this Expression expression)
   {
      var memberAccessList = new StringBuilder();
      VisitMemberAccessChain(expression, memberAccessList);

      return memberAccessList.ToString();
   }

   private static void VisitMemberAccessChain(Expression? expression, StringBuilder result)
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
      else if (expression is MethodCallExpression { Object: not null } methodCallExpression)
      {
         // Handle cases when the expression is a method call on a member (e.g. ToLower() for case-insensitive filtering)
         VisitMemberAccessChain(methodCallExpression.Object, result);
      }
   }
}
