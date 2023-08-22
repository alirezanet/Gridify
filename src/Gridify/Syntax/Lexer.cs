using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gridify.Syntax;

internal class Lexer
{
   private readonly List<string> _diagnostics = new();
   private readonly string _text;
   private readonly IEnumerable<IGridifyOperator> _customOperators;
   private int _position;
   private bool _waitingForValue;

   public Lexer(string text, IEnumerable<IGridifyOperator> customOperators)
   {
      _text = text;
      _customOperators = customOperators;
   }

   public IEnumerable<string> Diagnostics => _diagnostics;

   private char Current => _position >= _text.Length ? '\0' : _text[_position];

   private void Next()
   {
      _position++;
   }

   private char Peek(int offset)
   {
      return _position + offset >= _text.Length ? '\0' : _text[_position + offset];
   }

   public SyntaxToken NextToken()
   {
      if (_position >= _text.Length) return new SyntaxToken(SyntaxKind.End, _position, "\0");

      var peek = Peek(1);
      switch (Current)
      {
         case '(':
         {
            _waitingForValue = false;
            return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(");
         }
         case ')':
         {
            _waitingForValue = false;
            return new SyntaxToken(SyntaxKind.CloseParenthesis, _position++, ")");
         }
         case ',':
         {
            _waitingForValue = false;
            return new SyntaxToken(SyntaxKind.And, _position++, ",");
         }
         case '|':
         {
            _waitingForValue = false;
            return new SyntaxToken(SyntaxKind.Or, _position++, "|");
         }
         case '^':
         {
            _waitingForValue = true;
            return new SyntaxToken(SyntaxKind.StartsWith, _position++, "^");
         }
         case '$':
         {
            _waitingForValue = true;
            return new SyntaxToken(SyntaxKind.EndsWith, _position++, "$");
         }
         case '!' when peek == '^':
         {
            _waitingForValue = true;
            return new SyntaxToken(SyntaxKind.NotStartsWith, _position += 2, "!^");
         }
         case '!' when peek == '$':
         {
            _waitingForValue = true;
            return new SyntaxToken(SyntaxKind.NotEndsWith, _position += 2, "!$");
         }
         case '=' when peek == '*':
         {
            _waitingForValue = true;
            return new SyntaxToken(SyntaxKind.Like, _position += 2, "=*");
         }
         case '=':
         {
            _waitingForValue = true;
            return new SyntaxToken(SyntaxKind.Equal, _position++, "=");
         }
         case '!' when peek == '=':
         {
            _waitingForValue = true;
            return new SyntaxToken(SyntaxKind.NotEqual, _position += 2, "!=");
         }
         case '!' when peek == '*':
         {
            _waitingForValue = true;
            return new SyntaxToken(SyntaxKind.NotLike, _position += 2, "!*");
         }
         case '/' when peek == 'i':
            return new SyntaxToken(SyntaxKind.CaseInsensitive, _position += 2, "/i");
         case '<':
         {
            _waitingForValue = true;
            return peek == '='
               ? new SyntaxToken(SyntaxKind.LessOrEqualThan, _position += 2, "<=")
               : new SyntaxToken(SyntaxKind.LessThan, _position++, "<");
         }
         case '>':
         {
            _waitingForValue = true;
            return peek == '='
               ? new SyntaxToken(SyntaxKind.GreaterOrEqualThan, _position += 2, ">=")
               : new SyntaxToken(SyntaxKind.GreaterThan, _position++, ">");
         }
         case '#' when _customOperators.Any(): // Custom Operators
         {
            foreach (var cOp in _customOperators)
            {
               var op = cOp.GetOperator();
               if (op != _text.Substring(_position, op.Length)) continue;
               var start = _position;
               _position += op.Length;
               _waitingForValue = true;
               return new SyntaxToken(SyntaxKind.CustomOperator, start, op);
            }

            break;
         }
      }

      if (char.IsWhiteSpace(Current))
      {
         var start = _position;

         // skipper
         while (char.IsWhiteSpace(Current))
            Next();

         var length = _position - start;
         var text = _text.Substring(start, length);

         return new SyntaxToken(SyntaxKind.WhiteSpace, start, text);
      }

      if (TryToReadTheValue(out var valueToken)) return valueToken!;

      if (Current == '[')
      {
         Next();
         var start = _position;
         while (char.IsDigit(Current))
            Next();

         var length = _position - start;
         var text = _text.Substring(start, length);

         if (Current == ']')
         {
            _position++;
            return new SyntaxToken(SyntaxKind.FieldIndexToken, start, text);
         }

         _diagnostics.Add($"bad character input: '{peek.ToString()}' at {_position++.ToString()}. expected ']' ");
         return new SyntaxToken(SyntaxKind.BadToken, _position, Current.ToString());
      }

      if (char.IsLetter(Current) && !_waitingForValue)
      {
         var start = _position;

         while (char.IsLetterOrDigit(Current) || Current is '_' || Current is '.')
            Next();

         var length = _position - start;
         var text = _text.Substring(start, length);

         return new SyntaxToken(SyntaxKind.FieldToken, start, text);
      }

      _diagnostics.Add($"bad character input: '{Current.ToString()}' at {_position.ToString()}");
      return new SyntaxToken(SyntaxKind.BadToken, _position++, string.Empty);
   }

   private bool TryToReadTheValue(out SyntaxToken? valueToken)
   {
      if (_waitingForValue)
      {
         var start = _position;

         var exitCharacters = new[] { '(', ')', ',', '|' };

         var isPreviousEscapeChar = false;
         while (_position < _text.Length &&
                (!(Current == '/' && Peek(1) == 'i') ||
                 (Current == '/' && Peek(1) == 'i') && isPreviousEscapeChar)) // exit on case-insensitive operator
         {
            if (isPreviousEscapeChar)
            {
               isPreviousEscapeChar = false;
            }
            else if (Current == '\\')
            {
               isPreviousEscapeChar = true;
            }
            else if (exitCharacters.Contains(Current))
            {
               break;
            }

            Next();
         }

         var text = new StringBuilder();
         isPreviousEscapeChar = false;

         for (var i = start; i < _position; i++)
         {
            var current = _text[i];

            if (isPreviousEscapeChar)
            {
               text.Append(current);
               isPreviousEscapeChar = false;
            }
            else if (current == '\\')
            {
               isPreviousEscapeChar = true;
            }
            else
            {
               text.Append(current);
            }
         }


         _waitingForValue = false;
         {
            valueToken = new SyntaxToken(SyntaxKind.ValueToken, start, text.ToString());
            return true;
         }
      }

      valueToken = null;
      return false;
   }
}
