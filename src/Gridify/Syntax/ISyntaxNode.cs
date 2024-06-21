using System.Collections.Generic;

namespace Gridify.Syntax;

public interface ISyntaxNode
{
   public SyntaxKind Kind { get; }
   public IEnumerable<ISyntaxNode> GetChildren();
}
