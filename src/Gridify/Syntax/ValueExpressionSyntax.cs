using System.Collections.Generic;

namespace Gridify.Syntax
{
   internal sealed class ValueExpressionSyntax : ExpressionSyntax
   {
      public ValueExpressionSyntax(SyntaxToken valueToken)
      {
         ValueToken = valueToken;
      }

      public override SyntaxKind Kind => SyntaxKind.ValueExpression;

      public override IEnumerable<SyntaxNode> GetChildren()
      {
         yield return ValueToken;
      }

      public SyntaxToken ValueToken { get; }
   }
}