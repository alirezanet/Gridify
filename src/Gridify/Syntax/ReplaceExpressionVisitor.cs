using System.Linq.Expressions;

namespace Gridify.Syntax;

internal class ReplaceExpressionVisitor(Expression oldValue, Expression newValue) : ExpressionVisitor
{
   public override Expression Visit(Expression? node)
   {
      return node == oldValue ? newValue : base.Visit(node)!;
   }

   protected override Expression VisitUnary(UnaryExpression node)
   {
      if (node.Operand.Type == typeof(object) && node.Operand == oldValue)
      {
         return Expression.MakeUnary(node.NodeType, newValue, node.Type);
      }
      return base.VisitUnary(node);
   }
}
