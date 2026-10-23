using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests;

public class GridifyNestedCollectionNegationTests
{
   private readonly IQueryable<Order> _dataSource = new List<Order>
   {
      new() { Id = 1, Lines = [new() { Key = "color", Code = "A" }, new() { Key = "color", Code = "B" }], Tags = ["red", "blue"], Numbers = [1, 2] },
      new() { Id = 2, Lines = [new() { Key = "color", Code = "A" }, new() { Key = "size", Code = "B" }], Tags = ["red"], Numbers = [0] },
      new() { Id = 3, Lines = [], Tags = [], Numbers = [] },
      new() { Id = 4, Lines = null, Tags = null, Numbers = null },
      new() { Id = 5, Lines = [new() { Key = "color", Code = "" }, new() { Key = "color", Code = null }], Tags = [""], Numbers = [null] },
   }.AsQueryable();

   // text operators read the code, which is null in order 5
   private IQueryable<Order> WithoutNullCodes => _dataSource.Where(o => o.Id != 5);

   private static IGridifyMapper<Order> Mapper(bool collectionNegationAsNotAny = true) =>
      new GridifyMapper<Order>(q => q.CollectionNegationAsNotAny = collectionNegationAsNotAny)
         .AddMap("lineCode", o => o.Lines!.Select(l => l.Code))
         .AddMap("lines", (o, key) => o.Lines!.Where(l => l.Key == key).Select(l => l.Code))
         .AddMap("tags", o => o.Tags!.Select(t => t))
         .AddMap("numbers", o => o.Numbers!.Select(n => n));

   [Fact]
   public void ApplyFiltering_NotEqualOnCollection_ByDefault_MatchesWhenAnyElementDoesNotMatch()
   {
      var expected = _dataSource.Where(o => o.Lines != null && o.Lines.Any(l => l.Code != "A")).ToList();
      var actual = _dataSource.ApplyFiltering("lineCode!=A", Mapper(false)).ToList();

      Assert.Equal(expected, actual);
      Assert.Equal([1, 2, 5], actual.Select(o => o.Id));
   }

   [Fact]
   public void ApplyFiltering_NotEqualOnCollection_WithCollectionNegationAsNotAny_MatchesWhenNoElementMatches()
   {
      var expected = _dataSource.Where(o => !(o.Lines != null && o.Lines.Any(l => l.Code == "A"))).ToList();
      var actual = _dataSource.ApplyFiltering("lineCode!=A", Mapper()).ToList();

      Assert.Equal(expected, actual);
      Assert.Equal([3, 4, 5], actual.Select(o => o.Id));
   }

   [Theory]
   [InlineData("lineCode!*B")]
   [InlineData("lineCode!^B")]
   [InlineData("lineCode!$B")]
   public void ApplyFiltering_NegatedTextOperatorOnCollection_WithCollectionNegationAsNotAny_MatchesWhenNoElementMatches(string filter)
   {
      // one-letter codes: contains, starts with and ends with agree
      var expected = WithoutNullCodes.Where(o => !(o.Lines != null && o.Lines.Any(l => l.Code!.Contains("B")))).ToList();
      var actual = WithoutNullCodes.ApplyFiltering(filter, Mapper()).ToList();

      Assert.Equal(expected, actual);
      Assert.Equal([3, 4], actual.Select(o => o.Id));
   }

   [Fact]
   public void ApplyFiltering_EmptyValueOnCollection_ByDefault_MatchesWhenAnyElementIsEmpty()
   {
      var expected = _dataSource.Where(o => o.Lines != null && o.Lines.Any(l => string.IsNullOrEmpty(l.Code))).ToList();
      var actual = _dataSource.ApplyFiltering("lineCode=", Mapper(false)).ToList();

      Assert.Equal(expected, actual);
      Assert.Equal([5], actual.Select(o => o.Id));
   }

   [Fact]
   public void ApplyFiltering_EmptyValueOnCollection_WithCollectionNegationAsNotAny_MatchesWhenNoElementHasAValue()
   {
      var expected = _dataSource.Where(o => !(o.Lines != null && o.Lines.Any(l => !string.IsNullOrEmpty(l.Code)))).ToList();
      var actual = _dataSource.ApplyFiltering("lineCode=", Mapper()).ToList();

      Assert.Equal(expected, actual);
      Assert.Equal([3, 4, 5], actual.Select(o => o.Id));
   }

   [Theory]
   [InlineData("lineCode=A", new[] { 1, 2 })]
   [InlineData("lineCode!=", new[] { 1, 2 })] // has a value: not a negation of a collection condition
   [InlineData("lineCode=*B", new[] { 1, 2 })]
   [InlineData("lineCode^A", new[] { 1, 2 })]
   public void ApplyFiltering_NotNegatedConditionOnCollection_WithCollectionNegationAsNotAny_IsUnchanged(string filter, int[] expectedIds)
   {
      var expected = WithoutNullCodes.ApplyFiltering(filter, Mapper(false)).ToList();
      var actual = WithoutNullCodes.ApplyFiltering(filter, Mapper()).ToList();

      Assert.Equal(expected, actual);
      Assert.Equal(expectedIds, actual.Select(o => o.Id));
   }

   [Fact]
   public void ApplyFiltering_IndexedCollectionMapWithWhere_WithCollectionNegationAsNotAny_MatchesWhenNoElementOfThatKeyMatches()
   {
      var notA = _dataSource.ApplyFiltering("lines[color]!=A", Mapper()).ToList();
      var noValue = _dataSource.ApplyFiltering("lines[size]=", Mapper()).ToList();

      Assert.Equal(_dataSource.Where(o => !(o.Lines != null && o.Lines.Any(l => l.Key == "color" && l.Code == "A"))).ToList(), notA);
      Assert.Equal([3, 4, 5], notA.Select(o => o.Id));
      Assert.Equal(_dataSource.Where(o => !(o.Lines != null && o.Lines.Any(l => l.Key == "size" && !string.IsNullOrEmpty(l.Code)))).ToList(), noValue);
      Assert.Equal([1, 3, 4, 5], noValue.Select(o => o.Id));
   }

   [Fact]
   public void ApplyFiltering_NegatedConditionInParenthesisWithAnotherOnTheSameCollection_WithCollectionNegationAsNotAny_IsNotMergedIntoOneAny()
   {
      var expected = _dataSource.Where(o =>
         !(o.Lines != null && o.Lines.Any(l => l.Code == "C")) &&
         o.Lines != null && o.Lines.Any(l => l.Code == "A")).ToList();
      var actual = _dataSource.ApplyFiltering("(lineCode!=C, lineCode=A)", Mapper()).ToList();

      Assert.Equal(expected, actual);
      Assert.Equal([1, 2], actual.Select(o => o.Id));
   }

   [Fact]
   public void ApplyFiltering_NotEqualOnSimpleTypeCollection_WithCollectionNegationAsNotAny_IsTheSameAsByDefault()
   {
      // simple type collections already use !Contains for != (issue #204)
      var expected = _dataSource.Where(o => o.Tags == null || !o.Tags.Contains("red")).ToList();
      var byDefault = _dataSource.ApplyFiltering("tags!=red", Mapper(false)).ToList();
      var actual = _dataSource.ApplyFiltering("tags!=red", Mapper()).ToList();

      Assert.Equal(expected, byDefault);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void ApplyFiltering_EmptyValueOnSimpleTypeCollection_WithCollectionNegationAsNotAny_MatchesWhenNoElementHasAValue()
   {
      // as for a single value, an int? has a value when it isn't null (default(int?)), so 0 is a value
      var expected = _dataSource.Where(o => !(o.Numbers != null && o.Numbers.Any(n => n != null))).ToList();
      var actual = _dataSource.ApplyFiltering("numbers=", Mapper()).ToList();

      Assert.Equal(expected, actual);
      Assert.Equal([3, 4, 5], actual.Select(o => o.Id));
   }

   [Fact]
   public void ApplyFiltering_NotEqualOnCollection_WithCollectionNegationAsNotAnyAndDisableCollectionNullChecks_HasNoNullCheck()
   {
      IGridifyMapper<Order> mapper = new GridifyMapper<Order>(q =>
         {
            q.CollectionNegationAsNotAny = true;
            q.DisableCollectionNullChecks = true;
         })
         .AddMap("lineCode", o => o.Lines!.Select(l => l.Code));
      var withoutNullCollections = _dataSource.Where(o => o.Lines != null);

      var expected = withoutNullCollections.Where(o => !o.Lines!.Any(l => l.Code == "A")).ToList();
      var actual = withoutNullCollections.ApplyFiltering("lineCode!=A", mapper).ToList();

      Assert.Equal(expected, actual);
      Assert.Equal([3, 5], actual.Select(o => o.Id));
   }

   [Fact]
   public void GridifyMapperConfiguration_CollectionNegationAsNotAny_DefaultsToTheGlobalConfiguration()
   {
      Assert.False(new GridifyMapperConfiguration().CollectionNegationAsNotAny);

      GridifyGlobalConfiguration.CollectionNegationAsNotAny = true;
      try
      {
         Assert.True(new GridifyMapperConfiguration().CollectionNegationAsNotAny);
      }
      finally
      {
         GridifyGlobalConfiguration.CollectionNegationAsNotAny = false;
      }
   }

   public class Order
   {
      public int Id { get; set; }
      public List<Line>? Lines { get; set; }
      public List<string>? Tags { get; set; }
      public List<int?>? Numbers { get; set; }
   }

   public class Line
   {
      public string Key { get; set; } = string.Empty;
      public string? Code { get; set; }
   }
}
