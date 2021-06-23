using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.SymbolStore;
using System.Linq;

namespace Gridify.Syntax
{
   internal class Lexer
   {
      private readonly string _text;
      private int _position;
      private readonly List<string> _diagnostics = new List<string>();
      public IEnumerable<string> Diagnostics => _diagnostics;

      public Lexer(string text)
      {
         _text = text;
      }

      private char Current => _position >= _text.Length ? '\0' : _text[_position];
      private void Next() => _position++;
      private char Peek(int offset) => _position + offset >= _text.Length ? '\0' : _text[_position + offset];
      private bool _waitingForValue = false;

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
            case '=' when peek == '=':
               return new SyntaxToken(SyntaxKind.Equal, _position += 2, "==");
            case '=' when peek == '*':
               return new SyntaxToken(SyntaxKind.Like, _position += 2, "=*");
            case '!' when peek == '=':
               return new SyntaxToken(SyntaxKind.NotEqual, _position += 2, "!=");
            case '!' when peek == '*':
               return new SyntaxToken(SyntaxKind.NotLike, _position += 2, "!*");
            case '<' when peek == '<':
               return new SyntaxToken(SyntaxKind.LessThan, _position += 2, "<<");
            case '>' when peek == '>':
               return new SyntaxToken(SyntaxKind.GreaterThan, _position += 2, ">>");
            case '<' when peek == '=':
               return new SyntaxToken(SyntaxKind.LessOrEqualThan, _position += 2, "<=");
            case '>' when peek == '=':
               return new SyntaxToken(SyntaxKind.GreaterOrEqualThan, _position += 2, ">=");
         }

         if (char.IsLetter(Current) && !_waitingForValue)
         {
            var start = _position;

            while (char.IsLetterOrDigit(Current) || Current is '-' or '_')
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

            // TODO : we should provide scape character 
            var exitCharacters = new[] {'(', ')', ',', '|'};
            while (!exitCharacters.Contains(Current) && _position < _text.Length)
               Next();

            var length = _position - start;
            var text = _text.Substring(start, length);
            
            _waitingForValue = false;
            return new SyntaxToken(SyntaxKind.ValueToken, start, text);
         }

         _diagnostics.Add($"ERROR: bad character input: '{Current}' at {_position}");
         return new SyntaxToken(SyntaxKind.BadToken, _position++, string.Empty);
      }
   }
}