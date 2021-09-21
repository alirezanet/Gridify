using System.Collections.Generic;

namespace Gridify.Syntax
{
   internal sealed class ValueExpressionSyntax : ExpressionSyntax
   {
      public ValueExpressionSyntax(SyntaxToken valueToken, bool isCaseInsensitive)
      {
         ValueToken = valueToken;
         IsCaseInsensitive = isCaseInsensitive;
      }

      public override SyntaxKind Kind => SyntaxKind.ValueExpression;

      public override IEnumerable<SyntaxNode> GetChildren()
      {
         yield return ValueToken;
      }

      public SyntaxToken ValueToken { get; }
      public bool IsCaseInsensitive { get; }
   }
}