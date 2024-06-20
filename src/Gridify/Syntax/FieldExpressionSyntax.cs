using System.Collections.Generic;

namespace Gridify.Syntax;

internal sealed class FieldExpressionSyntax : ExpressionSyntax
{
   internal FieldExpressionSyntax(SyntaxToken fieldToken, string? indexer = default)
   {
      FieldToken = fieldToken;
      Indexer = indexer;
   }

   public override SyntaxKind Kind => SyntaxKind.FieldExpression;

   public override IEnumerable<SyntaxNode> GetChildren()
   {
      yield return FieldToken;
   }

   public string? Indexer { get; }
   public SyntaxToken FieldToken { get; }
}
