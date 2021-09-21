using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gridify.Syntax
{
   internal class Lexer
   {
      private readonly List<string> _diagnostics = new();
      private readonly string _text;
      private int _position;
      private bool _waitingForValue;

      public Lexer(string text)
      {
         _text = text;
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
               return new SyntaxToken(SyntaxKind.OpenParenthesisToken, _position++, "(");
            case ')':
               return new SyntaxToken(SyntaxKind.CloseParenthesis, _position++, ")");
            case ',':
               return new SyntaxToken(SyntaxKind.And, _position++, ",");
            case '|':
               return new SyntaxToken(SyntaxKind.Or, _position++, "|");
            case '^':
               return new SyntaxToken(SyntaxKind.StartsWith, _position++, "^");
            case '$':
               return new SyntaxToken(SyntaxKind.EndsWith, _position++, "$");
            case '!' when peek == '^':
               return new SyntaxToken(SyntaxKind.NotStartsWith, _position += 2, "!^");
            case '!' when peek == '$':
               return new SyntaxToken(SyntaxKind.NotEndsWith, _position += 2, "!$");
            case '=' when peek == '*':
               return new SyntaxToken(SyntaxKind.Like, _position += 2, "=*");
            case '=' :
               return new SyntaxToken(SyntaxKind.Equal, _position ++ , "=");
            case '!' when peek == '=':
               return new SyntaxToken(SyntaxKind.NotEqual, _position += 2, "!=");
            case '!' when peek == '*':
               return new SyntaxToken(SyntaxKind.NotLike, _position += 2, "!*");
            case '/' when peek == 'i':
               return new SyntaxToken(SyntaxKind.CaseInsensitive, _position += 2, "/i");
            case '<':
               return peek == '=' ? new SyntaxToken(SyntaxKind.LessOrEqualThan, _position += 2, "<=") :
                  new SyntaxToken(SyntaxKind.LessThan, _position++, "<");
            case '>':
               return peek == '=' ? new SyntaxToken(SyntaxKind.GreaterOrEqualThan, _position += 2, ">=") : 
                  new SyntaxToken(SyntaxKind.GreaterThan, _position++, ">");
         }

         if (char.IsLetter(Current) && !_waitingForValue)
         {
            var start = _position;

            while (char.IsLetterOrDigit(Current) || Current is '_')
               Next();

            var length = _position - start;
            var text = _text.Substring(start, length);

            _waitingForValue = true;
            return new SyntaxToken(SyntaxKind.FieldToken, start, text);
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

         if (_waitingForValue)
         {
            var start = _position;

            var exitCharacters = new[] {'(', ')', ',', '|'};
            var lastChar = '\0';
            while ((!exitCharacters.Contains(Current) || exitCharacters.Contains(Current) && lastChar == '\\') &&
                   _position < _text.Length &&
                   (!(Current == '/' && Peek(1) == 'i') || (Current == '/' && Peek(1) == 'i') && lastChar == '\\')) // exit on case-insensitive operator
            {
               lastChar = Current;
               Next();
            }

            var text = new StringBuilder();
            for (var i = start; i < _position; i++)
            {
               var current = _text[i];

               if (current != '\\') // ignore escape character
                  text.Append(current);
               else if (current == '\\' && i > 0) // escape escape character
                  if (_text[i - 1] == '\\')
                     text.Append(current);
            }

            _waitingForValue = false;
            return new SyntaxToken(SyntaxKind.ValueToken, start, text.ToString());
         }

         _diagnostics.Add($"bad character input: '{Current.ToString()}' at {_position.ToString()}");
         return new SyntaxToken(SyntaxKind.BadToken, _position++, string.Empty);
      }
   }
}