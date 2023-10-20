using System.Collections.Generic;
using System.Linq;

namespace Gridify.Syntax;

public class SyntaxToken : SyntaxNode
{
   public override SyntaxKind Kind { get; }
   public int Position { get; }
   public string Text { get; }

   public override IEnumerable<SyntaxNode> GetChildren()
   {
      return Enumerable.Empty<SyntaxNode>();
   }

   public SyntaxToken(SyntaxKind kind, int position, string text)
   {
      Kind = kind;
      Position = position;
      Text = text;
   }

   public SyntaxToken()
   {
      Text = string.Empty;
   }
}
