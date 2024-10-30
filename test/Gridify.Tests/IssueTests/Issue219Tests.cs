using System.Linq;
using FluentAssertions;
using Gridify.Syntax;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue219Tests
{
   [Fact]
   private void FieldNameWithDashShouldWork()
   {
      var syntaxTree = SyntaxTree.Parse("property-name = value");
      syntaxTree.Diagnostics.Should().BeEmpty();
      syntaxTree.Root.GetChildren().First().Should().BeOfType<FieldExpressionSyntax>();
   }
}
