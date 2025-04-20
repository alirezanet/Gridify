using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq.Expressions;

namespace Gridify.Reflection;

//https://stackoverflow.com/a/30489160/1698682
public class MemberNullPropagationVisitor : ExpressionVisitor
{
   protected override Expression VisitMember(MemberExpression node)
   {
      if (node.Expression == null || !IsNullable(node.Expression.Type))
         return base.VisitMember(node);

      if (node.Type.IsValueType && node.Type != typeof(Nullable<>))
      {
         return base.VisitMember(node);
      }
      var expression = base.Visit(node.Expression);
      var nullBaseExpression = Expression.Constant(null, expression.Type);
      var test = Expression.Equal(expression, nullBaseExpression);
      var nullMemberExpression = Expression.Constant(null, node.Type);
      return Expression.Condition(test, nullMemberExpression, node);
   }

   protected override Expression VisitMethodCall(MethodCallExpression node)
   {
      if (node.Object == null || !IsNullable(node.Object.Type))
         return base.VisitMethodCall(node);

      var expression = base.Visit(node.Object);
      var nullBaseExpression = Expression.Constant(null, expression.Type);
      var test = Expression.Equal(expression, nullBaseExpression);
      var nullMemberExpression = Expression.Constant(null, MakeNullable(node.Type));
      return Expression.Condition(test, nullMemberExpression, node);
   }

   private static Type MakeNullable(Type type)
   {
      if (IsNullable(type))
         return type;

      return typeof(Nullable<>).MakeGenericType(type);
   }

   private static bool IsNullable(Type type)
   {
      if (type.IsClass)
         return true;
      return type.IsGenericType &&
          type.GetGenericTypeDefinition() == typeof(Nullable<>);
   }
}
