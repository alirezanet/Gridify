using System.Collections.Generic;

namespace Gridify.Syntax;

internal sealed class ParenthesizedExpressionSyntax : ExpressionSyntax
{
   public SyntaxToken OpenParenthesisToken { get; }
   public ExpressionSyntax Expression { get; }
   public SyntaxToken CloseParenthesisToken { get; }
   public override SyntaxKind Kind  => SyntaxKind.ParenthesizedExpression;

   public ParenthesizedExpressionSyntax(SyntaxToken openParenthesisToken,ExpressionSyntax expression,SyntaxToken closeParenthesisToken)
   {
      OpenParenthesisToken = openParenthesisToken;
      Expression = expression;
      CloseParenthesisToken = closeParenthesisToken;
   }
   public override IEnumerable<SyntaxNode> GetChildren()
   {
      yield return OpenParenthesisToken;
      yield return Expression;
      yield return CloseParenthesisToken;
   }
}