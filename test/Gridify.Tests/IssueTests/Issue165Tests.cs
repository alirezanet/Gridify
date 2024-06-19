using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue165Tests
{
   [Fact]
   private void Filtering_WithDictionaryStringSubKey_ShouldReturnCorrectResult()
   {
      // arrange
      var ds = new List<TestModel>() { new() { Prop1 = new Dictionary<string, string> { { "subKey", "John" } } } }.AsQueryable();
      var expected = ds.Where(field => field.Prop1["subKey"] == "John");

      // act
      var queryBuilder = new QueryBuilder<TestModel>()
         .AddMap("prop1", (field, key) => field.Prop1[key])
         .AddCondition("prop1{subKey} = John");
      var actual = queryBuilder.Build(ds);

      // assert
      Assert.Equal(expected.ToString(), actual.ToString());
      Assert.NotEmpty(expected.ToList());
      Assert.Equal(expected.ToList().Count, actual.ToList().Count);
   }

   [Fact]
   private void Filtering_WithDictionaryIntSubKey_ShouldReturnCorrectResult()
   {
      // arrange
      var ds = new List<TestModel>() { new() { Prop2 = new Dictionary<int, string> { { 5987, "John" } } } }.AsQueryable();
      var expected = ds.Where(field => field.Prop2[5987] == "John");

      // act
      var queryBuilder = new QueryBuilder<TestModel>()
         .AddMap("prop2", (field, key) => field.Prop2[key])
         .AddCondition("prop2{5987} = John");
      var actual = queryBuilder.Build(ds);

      // assert
      Assert.Equal(expected.ToString(), actual.ToString());
      Assert.NotEmpty(expected.ToList());
      Assert.Equal(expected.ToList().Count, actual.ToList().Count);
   }

   [Fact]
   private void Filtering_WithDictionaryGuidSubKey_ShouldReturnTheCorrectResult()
   {
      // arrange
      const string id = "0f0ee7914ba7496aa13ae5ce4ff058b1";
      var ds = new List<TestModel>() { new() { Prop3 = new Dictionary<Guid, string> { { Guid.Parse(id), "John" } } } }.AsQueryable();
      var expected = ds.Where(field => field.Prop3[Guid.Parse(id)] == "John");

      // act
      var queryBuilder = new QueryBuilder<TestModel>()
         .AddMap("prop3", (field, key) => field.Prop3[Guid.Parse(key)])
         .AddCondition($"prop3{{{id}}} = John");
      var actual = queryBuilder.Build(ds);

      // assert
      Assert.NotEmpty(expected.ToList());
      Assert.Equal(expected.ToList().Count, actual.ToList().Count);
      Assert.Equal(expected.ToString(), actual.ToString());
   }

   private class TestModel
   {
      public string Id { get; set; } = Guid.NewGuid().ToString("N");
      public IDictionary<string, string> Prop1 { get; set; } = new Dictionary<string, string>();
      public IDictionary<int, string> Prop2 { get; set; } = new Dictionary<int, string>();
      public IDictionary<Guid, string> Prop3 { get; set; } = new Dictionary<Guid, string>();
   }
}

