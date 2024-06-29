using System.Collections.Generic;

namespace Gridify.Syntax;

public sealed class ValueExpressionSyntax(SyntaxToken valueToken, bool isCaseInsensitive, bool isNullOrDefault) : ExpressionSyntax
{
   public override SyntaxKind Kind => SyntaxKind.ValueExpression;

   public override IEnumerable<ISyntaxNode> GetChildren()
   {
      yield return ValueToken;
   }

   public SyntaxToken ValueToken { get; } = valueToken;
   public bool IsCaseInsensitive { get; } = isCaseInsensitive;
   public bool IsNullOrDefault { get; } = isNullOrDefault;
}
