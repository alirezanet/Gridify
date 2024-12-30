using System.Linq;
using Xunit;
// ReSharper disable InconsistentNaming

namespace Gridify.Tests.IssueTests;

public class Issue242And208
{
   [Fact]
   private void FilteringEquals_WithEscapedLeadingSpace_ShouldNotBeIgnored()
   {
      // arrange
      var ds = GridifyExtensionsShould.GetSampleData().AsQueryable();
      var expected = ds.Where(__TestClass => __TestClass.Name == " test");

      // act
      var actual = ds.ApplyFiltering(@"name=\ test");

      // assert
      Assert.Equal(expected.ToString(), actual.ToString());
   }

   [Fact]
   private void FilteringContains_WithEscapedLeadingSpace_ShouldNotBeIgnored()
   {
      // arrange
      var ds = GridifyExtensionsShould.GetSampleData().AsQueryable();
      var expected = ds.Where(__TestClass => __TestClass.Name!.Contains(" test"));

      // act
      var actual = ds.ApplyFiltering(@"name=*\ test");

      // assert
      Assert.Equal(expected.ToString(), actual.ToString());
   }
}
