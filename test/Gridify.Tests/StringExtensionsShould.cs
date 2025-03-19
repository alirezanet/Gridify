#nullable enable
using Gridify.Syntax;

using System.Linq;

using Xunit;

namespace Gridify.Tests;

public class StringExtensionsShould
{
   [Fact]
   public void ParseFilterings_MembersAndIndexerIsSet()
   {
      var filterings = "name = Jack, arrayProp[8] > 10, dictProp[name] = John";

      var actual = filterings.ParseFilterings()
         .ToList();

      Assert.Equal(3, actual.Count);

      Assert.Equal("name", actual[0].MemberName);
      Assert.Null(actual[0].Indexer);

      Assert.Equal("arrayProp", actual[1].MemberName);
      Assert.Equal("8", actual[1].Indexer);

      Assert.Equal("dictProp", actual[2].MemberName);
      Assert.Equal("name", actual[2].Indexer);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   public void ParseFilterings_ShouldReturnEmtpy(string? filterings)
   {
      var actual = filterings!.ParseFilterings()
         .ToList();

      Assert.Empty(actual);
   }

   [Fact]
   public void ParseOrderings_MembersAndOrderingIsSet()
   {
      var orderings = "name, firstName desc, lastName asc";

      var actual = orderings.ParseOrderings()
         .ToList();

      Assert.Equal(3, actual.Count);

      Assert.Equal("name", actual[0].MemberName);
      Assert.Equal(OrderingType.Normal, actual[0].OrderingType);
      Assert.True(actual[0].IsAscending);

      Assert.Equal("firstName", actual[1].MemberName);
      Assert.Equal(OrderingType.Normal, actual[1].OrderingType);
      Assert.False(actual[1].IsAscending);

      Assert.Equal("lastName", actual[2].MemberName);
      Assert.Equal(OrderingType.Normal, actual[2].OrderingType);
      Assert.True(actual[2].IsAscending);
   }

   [Theory]
   [InlineData(null)]
   [InlineData("")]
   public void ParseOrderings_ShouldReturnEmtpy(string? orderings)
   {
      var actual = orderings!.ParseOrderings()
         .ToList();

      Assert.Empty(actual);
   }
}
