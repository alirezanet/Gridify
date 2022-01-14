using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Gridify.Tests")]

namespace Gridify.Syntax;

public sealed class SyntaxTree
{
   public IReadOnlyList<string> Diagnostics { get; }
   public ExpressionSyntax Root { get; }

   public SyntaxTree(IEnumerable<string> diagnostics, ExpressionSyntax root, SyntaxToken endToken)
   {
      Diagnostics = diagnostics.ToArray();
      Root = root;
   }

   public static SyntaxTree Parse(string text, IEnumerable<IGridifyOperator> customOperators)
   {
      var parser = new Parser(text, customOperators);
      return parser.Parse();
   }
}
