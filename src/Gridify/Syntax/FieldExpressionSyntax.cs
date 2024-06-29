using System.Collections.Generic;

namespace Gridify.Syntax;

public sealed class FieldExpressionSyntax(SyntaxToken fieldToken, string? indexer = default) : ExpressionSyntax
{
   public override SyntaxKind Kind => SyntaxKind.FieldExpression;

   public override IEnumerable<ISyntaxNode> GetChildren()
   {
      yield return FieldToken;
   }

   public string? Indexer { get; } = indexer;
   public SyntaxToken FieldToken { get; } = fieldToken;
}
