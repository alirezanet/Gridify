using System.Collections.Generic;

namespace Gridify.Syntax;

internal sealed class FieldExpressionSyntax : ExpressionSyntax
{
   internal FieldExpressionSyntax(SyntaxToken fieldToken)
   {
      FieldToken = fieldToken;
   }

   public FieldExpressionSyntax(SyntaxToken fieldToken, int index)
   {
      IsCollection = true;
      Index = index;
      FieldToken = fieldToken;
   }

   public override SyntaxKind Kind => SyntaxKind.FieldExpression;

   public override IEnumerable<SyntaxNode> GetChildren()
   {
      yield return FieldToken;
   }

   public bool IsCollection { get; }
   public int Index { get; }
   public SyntaxToken FieldToken { get; }
}