using System.Collections.Generic;
using System.Linq;

namespace Gridify.Syntax;

internal struct Parser
{
   private List<string>? _diagnostics = null;
   private readonly List<SyntaxToken> _tokens = [];
   private int _position;

   private static bool IsOperator(SyntaxKind kind)
   {
      return kind is SyntaxKind.Equal or
         SyntaxKind.Like or
         SyntaxKind.NotEqual or
         SyntaxKind.NotLike or
         SyntaxKind.GreaterThan or
         SyntaxKind.LessThan or
         SyntaxKind.GreaterOrEqualThan or
         SyntaxKind.LessOrEqualThan or
         SyntaxKind.StartsWith or
         SyntaxKind.EndsWith or
         SyntaxKind.NotStartsWith or
         SyntaxKind.NotEndsWith or
         SyntaxKind.CustomOperator;
   }

   public Parser(string text, IEnumerable<IGridifyOperator> customOperators)
   {
      var lexer = new Lexer(text, customOperators);
      SyntaxToken token;
      do
      {
         token = lexer.NextToken();
         if (token.Kind != SyntaxKind.WhiteSpace) _tokens.Add(token);
      } while (token.Kind != SyntaxKind.End);

      if (lexer.Diagnostics.Any())
         AddDiagnostics(lexer.Diagnostics);
   }

   private SyntaxToken Current => Peek(0);

   private SyntaxToken Peek(int offset)
   {
      var index = _position + offset;
      return index >= _tokens.Count ? _tokens[_tokens.Count - 1] : _tokens[index];
   }

   public SyntaxTree Parse()
   {
      var expression = ParseTerm();
      var end = Match(SyntaxKind.End, GetExpectation(expression.Kind));
      return new SyntaxTree(_diagnostics ?? Enumerable.Empty<string>(), expression, end);
   }

   private SyntaxKind GetExpectation(SyntaxKind kind)
   {
      switch (kind)
      {
         case SyntaxKind.FieldExpression or SyntaxKind.FieldToken:
            return SyntaxKind.Operator;
         case SyntaxKind.ValueExpression or SyntaxKind.ValueToken:
            return SyntaxKind.End;
         case SyntaxKind.And or SyntaxKind.Or:
            return SyntaxKind.FieldToken;
         default:
         {
            if (IsOperator(kind) || kind == SyntaxKind.BinaryExpression)
               return SyntaxKind.ValueToken;

            return SyntaxKind.End;
         }
      }
   }

   private ExpressionSyntax ParseTerm()
   {
      var left = ParseFactor();

      while (Current.Kind is SyntaxKind.And or SyntaxKind.Or)
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

      while (IsOperator(Current.Kind))
      {
         var operatorToken = NextToken();
         var right = ParseValueExpression();
         left = new BinaryExpressionSyntax(left, operatorToken, right);
      }

      return left;
   }

   private ExpressionSyntax ParseValueExpression()
   {
      // field=
      if (Current.Kind != SyntaxKind.ValueToken)
         return new ValueExpressionSyntax(new SyntaxToken(), false, true);

      var valueToken = Match(SyntaxKind.ValueToken);
      var isCaseInsensitive = TryMatch(SyntaxKind.CaseInsensitive, out _);
      return new ValueExpressionSyntax(valueToken, isCaseInsensitive, false);
   }

   private SyntaxToken NextToken()
   {
      var current = Current;
      _position++;
      return current;
   }

   private bool TryMatch(SyntaxKind kind, out SyntaxToken token)
   {
      if (Current.Kind != kind)
      {
         token = Current;
         return false;
      }

      token = NextToken();
      return true;
   }

   private SyntaxToken Match(SyntaxKind kind, SyntaxKind? expectation = null)
   {
      if (Current.Kind == kind || expectation == SyntaxKind.ValueToken && Current.Kind == SyntaxKind.CaseInsensitive)
         return NextToken();

      expectation ??= kind;

      if (_diagnostics != null && !_diagnostics.Any(q => q.StartsWith("Unexpected token")))
         AddDiagnostics($"Unexpected token <{Current.Kind}> at index {Current.Position}, expected <{expectation}>");

      return new SyntaxToken(kind, Current.Position, Current.Text);
   }

   private ExpressionSyntax ParsePrimaryExpression()
   {
      if (Current.Kind != SyntaxKind.OpenParenthesisToken) return ParseFieldExpression();

      var left = NextToken();
      var expression = ParseTerm();
      var right = Match(SyntaxKind.CloseParenthesis);
      return new ParenthesizedExpressionSyntax(left, expression, right);
   }

   private ExpressionSyntax ParseFieldExpression()
   {
      var fieldToken = Match(SyntaxKind.FieldToken);

      return TryMatch(SyntaxKind.FieldIndexer, out var fieldIndexer)
         ? new FieldExpressionSyntax(fieldToken, fieldIndexer.Text)
         : new FieldExpressionSyntax(fieldToken);
   }
   private void AddDiagnostics(string message)
   {
      _diagnostics ??= [];
      _diagnostics.Add(message);
   }
   private void AddDiagnostics(IEnumerable<string> messages)
   {
      _diagnostics ??= [];
      _diagnostics.AddRange(messages);
   }
}
