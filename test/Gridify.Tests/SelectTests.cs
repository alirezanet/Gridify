using System;
using System.Collections.Generic;
using System.Linq;
using Gridify;
using Gridify.Syntax;
using Xunit;

namespace Gridify.Tests;

public class SelectTokenizerTests
{
   [Fact]
   public void Parse_SingleField_ReturnsOnePath()
   {
      var result = SelectTokenizer.Parse("name");
      Assert.Equal(new[] { "name" }, result);
   }

   [Fact]
   public void Parse_MultipleFields_ReturnsList()
   {
      var result = SelectTokenizer.Parse("name,age,tag");
      Assert.Equal(new[] { "name", "age", "tag" }, result);
   }

   [Fact]
   public void Parse_TrimsWhitespaceAroundEachToken()
   {
      var result = SelectTokenizer.Parse(" name , age ,  tag ");
      Assert.Equal(new[] { "name", "age", "tag" }, result);
   }

   [Fact]
   public void Parse_DottedPath_PreservedIntact()
   {
      var result = SelectTokenizer.Parse("address.city,orders.amount");
      Assert.Equal(new[] { "address.city", "orders.amount" }, result);
   }

   [Fact]
   public void Parse_DuplicateTokens_AreDeduplicated()
   {
      var result = SelectTokenizer.Parse("name,age,name,age");
      Assert.Equal(new[] { "name", "age" }, result);
   }

   [Fact]
   public void Parse_NullOrWhitespace_ReturnsEmpty()
   {
      Assert.Empty(SelectTokenizer.Parse(null));
      Assert.Empty(SelectTokenizer.Parse(""));
      Assert.Empty(SelectTokenizer.Parse("   "));
   }

   [Theory]
   [InlineData(",")]
   [InlineData("name,,age")]
   [InlineData(",name")]
   [InlineData("name,")]
   [InlineData(".")]
   [InlineData(".name")]
   [InlineData("name.")]
   [InlineData("name..age")]
   [InlineData("1name")]
   [InlineData("name space")]
   [InlineData("name-bad")]
   public void Parse_InvalidToken_Throws(string input)
   {
      Assert.Throws<GridifySelectException>(() => SelectTokenizer.Parse(input));
   }
}
