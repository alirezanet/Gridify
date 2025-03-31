using System;
using System.Collections.Generic;
using System.Linq;

namespace Gridify.Syntax
{
    public static class SyntaxNodeExtensions
    {
      private static readonly FieldExpressionComparer FieldExpressionComparer = new();

      /// <summary>
      /// Retrieves all distinct field expressions from the specified syntax node.
      /// </summary>
      /// <param name="syntaxNode">The syntax node from which to retrieve descendant field expressions.</param>
      /// <returns>An collection of <see cref="FieldExpressionSyntax"/> instances representing the descendant field expressions.</returns>
      public static IEnumerable<FieldExpressionSyntax> DistinctFieldExpressions(this ISyntaxNode syntaxNode)
      {
         if (syntaxNode is null) throw new ArgumentNullException(nameof(syntaxNode));

         var fieldExpressions = syntaxNode.Descendants()
            .OfType<FieldExpressionSyntax>()
            .Distinct(FieldExpressionComparer);

         return fieldExpressions;
      }

      /// <summary>
      /// Retrieves all descendants from the specified syntax node.
      /// </summary>
      /// <param name="syntaxNode">The syntax node from which to retrieve descendant syntax nodes.</param>
      /// <returns>An collection of <see cref="ISyntaxNode"/> instances representing the descendant syntax nodes.</returns>
      public static IEnumerable<ISyntaxNode> Descendants(this ISyntaxNode syntaxNode)
      {
         if (syntaxNode is null) throw new ArgumentNullException(nameof(syntaxNode));

         var nodes = new Stack<ISyntaxNode>([syntaxNode]);
         while (nodes.Count != 0)
         {
            var node = nodes.Pop();
            yield return node;
            foreach (var n in node.GetChildren()) nodes.Push(n);
         }
      }
   }
}
