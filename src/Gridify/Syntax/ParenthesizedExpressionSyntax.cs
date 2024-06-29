using System.Collections.Generic;

namespace Gridify.Syntax;

public sealed class ParenthesizedExpressionSyntax(SyntaxToken openParenthesisToken, ExpressionSyntax expression, SyntaxToken closeParenthesisToken)
   : ExpressionSyntax
{
   public SyntaxToken OpenParenthesisToken { get; } = openParenthesisToken;
   public ExpressionSyntax Expression { get; } = expression;
   public SyntaxToken CloseParenthesisToken { get; } = closeParenthesisToken;
   public override SyntaxKind Kind => SyntaxKind.ParenthesizedExpression;

   public override IEnumerable<ISyntaxNode> GetChildren()
   {
      yield return OpenParenthesisToken;
      yield return Expression;
      yield return CloseParenthesisToken;
   }
}
