using System.Collections.Generic;

namespace Gridify.Syntax
{
   internal sealed class FieldExpressionSyntax : ExpressionSyntax
   {
      internal FieldExpressionSyntax(SyntaxToken fieldToken)
      {
         FieldToken = fieldToken;
      }

      public override SyntaxKind Kind => SyntaxKind.FieldExpression;

      public override IEnumerable<SyntaxNode> GetChildren()
      {
         yield return FieldToken;
      }

      public SyntaxToken FieldToken { get; }
   }
}