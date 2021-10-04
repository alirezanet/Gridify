using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gridify.Syntax
{
   internal sealed class FieldExpressionSyntax : ExpressionSyntax
   {
      internal FieldExpressionSyntax(SyntaxToken fieldToken)
      {
         // for performance reason we simply check the last character first 
         if (fieldToken.Text.EndsWith("]"))
         {
            // checking indexes from the field names
            var regex = new Regex(@"(\w+)\[(\d+)\]");
            var match = regex.Match(fieldToken.Text);
            if (!match.Success) throw new ArgumentException($"Invalid filed name '{fieldToken.Text}'");
            IsCollection = true;
            Index = int.Parse(match.Groups[2].Value);
            FieldToken = new SyntaxToken(SyntaxKind.FieldToken, 0, match.Groups[1].Value);
         }
         else
            FieldToken = fieldToken;
      }


      public override SyntaxKind Kind => SyntaxKind.FieldExpression;

      public override IEnumerable<SyntaxNode> GetChildren()
      {
         yield return FieldToken;
      }

      public bool IsCollection { get; }
      public int Index { get; }
      public SyntaxToken FieldToken { get; }
   }
}