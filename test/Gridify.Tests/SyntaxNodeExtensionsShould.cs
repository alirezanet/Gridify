#nullable enable
using Gridify.Syntax;

using System;
using System.Linq;

using Xunit;

namespace Gridify.Tests;

public class SyntaxNodeExtensionsShould
{
   [Fact]
   public void Descendants_ShouldReturnSyntaxNodes()
   {
      var filterings = "name = Jack, arrayProp[8] > 10, dictProp[name] = John";

      var syntaxNodes = SyntaxTree.Parse(filterings).Root.Descendants().ToList();

      Assert.Equal(22, syntaxNodes.Count);
   }

   [Fact]
   public void Descendants_WhenNull_ShouldThrow()
   {
      ISyntaxNode syntaxNode = null!;

      var act = () => syntaxNode.Descendants().ToList();

      Assert.Throws<ArgumentNullException>(act);
   }

   [Fact]
   public void DistinctFieldExpressions_ShouldReturnFieldExpressions()
   {
      var filterings = "name = Jack, arrayProp[8] > 10, dictProp[name] = John";

      var syntaxNodes = SyntaxTree.Parse(filterings).Root.DistinctFieldExpressions().ToList();

      Assert.Equal(3, syntaxNodes.Count);
   }

   [Fact]
   public void DistinctFieldExpressions_WhenNull_ShouldThrow()
   {
      ISyntaxNode syntaxNode = null!;

      var act = () => syntaxNode.DistinctFieldExpressions().ToList();

      Assert.Throws<ArgumentNullException>(act);

   }
}
