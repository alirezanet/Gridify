using System.Collections.Generic;
using System.Linq;

namespace Gridify.Syntax
{
   internal class Parser
   {
      private readonly List<string> _diagnostics = new();
      private readonly SyntaxToken[] _tokens;
      private int _position;

      public Parser(string text)
      {
         var tokens = new List<SyntaxToken>();
         var lexer = new Lexer(text);
         SyntaxToken token;
         do
         {
            token = lexer.NextToken();
            if (token.Kind != SyntaxKind.BadToken && token.Kind != SyntaxKind.WhiteSpace) tokens.Add(token);
         } while (token.Kind != SyntaxKind.End);

         _tokens = tokens.ToArray();
         _diagnostics.AddRange(lexer.Diagnostics);
      }

      private SyntaxToken Current => Peek(0);

      private SyntaxToken Peek(int offset)
      {
         var index = _position + offset;
         return index >= _tokens.Length ? _tokens[_tokens.Length - 1] : _tokens[index];
      }

      public SyntaxTree Parse()
      {
         var expression = ParseTerm();
         var end = Match(SyntaxKind.End);
         return new SyntaxTree(_diagnostics, expression, end);
      }

      private ExpressionSyntax ParseTerm()
      {
         var left = ParseFactor();
         var binaryKinds = new[]
         {
            SyntaxKind.And,
            SyntaxKind.Or
         };

         while (binaryKinds.Contains(Current.Kind))
         {
            var operatorToken = NextToken();
            var right = ParseFactor();
            left = new BinaryExpressionSyntax(left, operatorToken, right);
         }

         return left;
      }

      private ExpressionSyntax ParseFactor()
      {
         var left = ParsePrimaryExpression();
         var binaryKinds = new[]
         {
            SyntaxKind.Equal,
            SyntaxKind.Like,
            SyntaxKind.NotEqual,
            SyntaxKind.NotLike,
            SyntaxKind.GreaterThan,
            SyntaxKind.LessThan,
            SyntaxKind.GreaterOrEqualThan,
            SyntaxKind.LessOrEqualThan,
            SyntaxKind.StartsWith,
            SyntaxKind.EndsWith,
            SyntaxKind.NotStartsWith,
            SyntaxKind.NotEndsWith
         };

         while (binaryKinds.Contains(Current.Kind))
         {
            var operatorToken = NextToken();
            var right = ParseValueExpression();
            left = new BinaryExpressionSyntax(left, operatorToken, right);
         }

         return left;
      }

      private ExpressionSyntax ParseValueExpression()
      {
         var valueToken = Match(SyntaxKind.ValueToken);
         return new ValueExpressionSyntax(valueToken);
      }

      private SyntaxToken NextToken()
      {
         var current = Current;
         _position++;
         return current;
      }

      private SyntaxToken Match(SyntaxKind kind)
      {
         if (Current.Kind == kind)
            return NextToken();

         _diagnostics.Add($"Unexpected token <{Current.Kind}>, expected <{kind}>");
         return new SyntaxToken(kind, Current.Position, string.Empty);
      }

      private ExpressionSyntax ParsePrimaryExpression()
      {
         if (Current.Kind == SyntaxKind.OpenParenthesisToken)
         {
            var left = NextToken();
            var expression = ParseTerm();
            var right = Match(SyntaxKind.CloseParenthesis);
            return new ParenthesizedExpressionSyntax(left, expression, right);
         }

         var fieldToken = Match(SyntaxKind.FieldToken);
         return new FieldExpressionSyntax(fieldToken);
      }
   }
}