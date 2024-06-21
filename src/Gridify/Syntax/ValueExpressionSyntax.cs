using System.Collections.Generic;

namespace Gridify.Syntax;

internal sealed class ValueExpressionSyntax : ExpressionSyntax
{
   public ValueExpressionSyntax(SyntaxToken valueToken, bool isCaseInsensitive, bool isNullOrDefault)
   {
      ValueToken = valueToken;
      IsCaseInsensitive = isCaseInsensitive;
      IsNullOrDefault = isNullOrDefault;
   }

   public override SyntaxKind Kind => SyntaxKind.ValueExpression;

   public override IEnumerable<ISyntaxNode> GetChildren()
   {
      yield return ValueToken;
   }

   public SyntaxToken ValueToken { get; }
   public bool IsCaseInsensitive { get; }
   public bool IsNullOrDefault { get; }
}
