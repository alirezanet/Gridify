using System.Collections.Generic;
using System.Linq;

namespace Gridify.Syntax;

public struct SyntaxToken(SyntaxKind kind, int position, string text) : ISyntaxNode
{
   public int Position { get; } = position;
   public string Text { get; } = text;
   public SyntaxKind Kind { get; } = kind;

   public IEnumerable<ISyntaxNode> GetChildren()
   {
      return Enumerable.Empty<ISyntaxNode>();
   }

   public SyntaxToken() : this(SyntaxKind.End, 0, string.Empty)
   {
   }
}
