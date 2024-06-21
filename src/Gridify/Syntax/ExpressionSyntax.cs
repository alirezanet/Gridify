using System.Collections.Generic;

namespace Gridify.Syntax;

public abstract class ExpressionSyntax : ISyntaxNode
{
   public abstract SyntaxKind Kind { get; }
   public abstract IEnumerable<ISyntaxNode> GetChildren();
}
