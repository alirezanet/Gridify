using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Gridify.Syntax;

public ref struct Lexer(string text, IEnumerable<IGridifyOperator> customOperators)
{
   private List<string>? _diagnostics = null;
   private readonly ReadOnlySpan<char> _text = text.AsSpan();
   private int _position;
   private bool _waitingForValue;

   public IEnumerable<string> Diagnostics => _diagnostics ?? Enumerable.Empty<string>();

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
         case '#' when customOperators.Any(): // Custom Operators
         {
            foreach (var cOp in customOperators)
            {
               var op = cOp.GetOperator();
               var opSlice = op.AsSpan();
               var sliceSize = Math.Min(opSlice.Length, _text.Length - _position - 1);
               var textSlice = _text.Slice(_position, sliceSize);
               if (!opSlice.SequenceEqual(textSlice)) continue;

               var start = _position;
               _position += opSlice.Length;
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
         var text = _text.Slice(start, length);

         return new SyntaxToken(SyntaxKind.WhiteSpace, start, text.ToString());
      }

      if (TryToReadTheValue(out var valueToken)) return valueToken!;
      if (TryParseIndexer(peek, out var fieldIndexToken)) return fieldIndexToken;

      if (char.IsLetter(Current) && !_waitingForValue)
      {
         var start = _position;

         while (char.IsLetterOrDigit(Current) || Current is '_' || Current is '.' || Current is '-')
            Next();

         var length = _position - start;
         var text = _text.Slice(start, length);

         return new SyntaxToken(SyntaxKind.FieldToken, start, text.ToString());
      }

      AddDiagnostics($"bad character input: '{Current.ToString()}', at index {_position.ToString()}");
      return new SyntaxToken(SyntaxKind.BadToken, _position++, string.Empty);
   }

   private bool TryParseIndexer(char peek, out SyntaxToken nextToken)
   {
      if (Current == '[')
      {
         Next();
         var start = _position;
         while (Current != ']' && _position < _text.Length)
            Next();

         var length = _position - start;
         var text = _text.Slice(start, length);

         if (Current == ']')
         {
            _position++;
            {
               nextToken = new SyntaxToken(SyntaxKind.FieldIndexer, start, text.ToString());
               return true;
            }
         }

         AddDiagnostics($"Indexer is not closed: '{peek.ToString()}' at {_position++.ToString()}. expected ']' ");
         {
            nextToken = new SyntaxToken(SyntaxKind.BadToken, _position, Current.ToString());
            return true;
         }
      }
      nextToken = default!;
      return false;
   }

   private bool TryToReadTheValue(out SyntaxToken valueToken)
   {
      if (_waitingForValue)
      {
         var start = _position;

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
            else if (Current is '(' or ')' or ',' or '|')
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

      valueToken = new SyntaxToken();
      return false;
   }

   private void AddDiagnostics(string message)
   {
      _diagnostics ??= [];
      _diagnostics.Add(message);
   }
}
