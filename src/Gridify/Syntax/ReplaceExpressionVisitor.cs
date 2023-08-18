using System.Linq.Expressions;

namespace Gridify.Syntax;

internal class ReplaceExpressionVisitor : ExpressionVisitor
{
   private readonly Expression _oldValue;
   private readonly Expression _newValue;

   public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
   {
      _oldValue = oldValue;
      _newValue = newValue;
   }

   public override Expression Visit(Expression? node)
   {
      return node == _oldValue ? _newValue : base.Visit(node)!;
   }

   protected override Expression VisitUnary(UnaryExpression node)
   {
      if (node.Operand.Type == typeof(object) && node.Operand == _oldValue)
      {
         return Expression.MakeUnary(node.NodeType, _newValue, node.Type);
      }
      return base.VisitUnary(node);
   }
}
