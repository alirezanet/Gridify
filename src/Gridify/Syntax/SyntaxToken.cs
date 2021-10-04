using System.Collections.Generic;
using System.Linq;

namespace Gridify.Syntax
{
   public class SyntaxToken : SyntaxNode
   {
      public override SyntaxKind Kind { get; }
      public string Text { get; }

      public override IEnumerable<SyntaxNode> GetChildren()
      {
         return Enumerable.Empty<SyntaxNode>();
      }

      // we don't need position yet ( we can use the second argument later when we had a analyzer or debugger )
      public SyntaxToken(SyntaxKind kind, int _ ,  string text)
      {
         Kind = kind;
         Text = text;
      }

      public SyntaxToken()
      {
         Text = string.Empty;
      }
   }
}