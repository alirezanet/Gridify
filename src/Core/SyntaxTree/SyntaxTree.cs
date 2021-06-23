using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Core.Tests")]
namespace Gridify.Syntax
{
   public sealed class SyntaxTree
   {
      public IReadOnlyList<string> Diagnostics { get; }
      public ExpressionSyntax Root { get; }
      private SyntaxToken EndToken { get; }

      public SyntaxTree(IEnumerable<string> diagnostics, ExpressionSyntax root,SyntaxToken endToken)
      {
         Diagnostics = diagnostics.ToArray();
         Root = root;
         EndToken = endToken;
      }

      public static SyntaxTree Parse(string text)
      {
         var parser = new Parser(text);
         return parser.Parse();
      }
   }
}