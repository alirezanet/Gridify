using System.Collections.Generic;
using System.Linq;

namespace Gridify.Syntax;

public sealed class SyntaxTree(IEnumerable<string> diagnostics, ExpressionSyntax root)
{
   public IEnumerable<string> Diagnostics { get; } = diagnostics;
   public ExpressionSyntax Root { get; } = root;


   public static SyntaxTree Parse(string text, IEnumerable<IGridifyOperator> customOperators)
   {
      var parser = new Parser(text, customOperators);
      return parser.Parse();
   }
}
