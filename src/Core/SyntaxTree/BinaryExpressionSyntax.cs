using System.Collections.Generic;

namespace Gridify.Syntax
{
   internal sealed class BinaryExpressionSyntax : ExpressionSyntax
   {
      public ExpressionSyntax Left { get; }
      public SyntaxToken OperatorToken { get; }
      public ExpressionSyntax Right { get; }

      public BinaryExpressionSyntax(ExpressionSyntax left, SyntaxToken operatorToken, ExpressionSyntax right)
      {
         Left = left;
         OperatorToken = operatorToken;
         Right = right;
      }

      public override SyntaxKind Kind => SyntaxKind.BinaryExpression;

      public override IEnumerable<SyntaxNode> GetChildren()
      {
         yield return Left;
         yield return OperatorToken;
         yield return Right;
      }
   }
}