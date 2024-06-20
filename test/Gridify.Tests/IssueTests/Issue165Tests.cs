using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue165Tests
{
   [Fact]
   private void Filtering_WithDictionaryStringSubKey_ShouldReturnCorrectResult()
   {
      // arrange
      var ds = new List<TestModel>() { new() { Prop1 = new Dictionary<string, string> { { "subKey", "John" } } } }.AsQueryable();
      var expected = ds.Where(field => field.Prop1.ContainsKey("subKey") && field.Prop1["subKey"] == "John");

      // act
      var queryBuilder = new QueryBuilder<TestModel>()
         .AddMap("prop1", (field, key) => field.Prop1[key])
         .AddCondition("prop1[subKey] = John");
      var actual = queryBuilder.Build(ds);

      // assert
      Assert.Equal(expected.ToString(), actual.ToString());
      Assert.NotEmpty(expected.ToList());
      Assert.Equal(expected.ToList().Count, actual.ToList().Count);
   }

   [Fact]
   private void FilteringWithDictionaryStringSubKey_WhenKeyDoesNotExists_ShouldNotThrowException()
   {
      // arrange
      var ds = new List<TestModel>() { new() { Prop1 = new Dictionary<string, string> { { "key1", "John" } } } }.AsQueryable();
      var expected = ds.Where(field => field.Prop1.ContainsKey("key2") && field.Prop1["key2"] == "John");
      var x = expected.ToList();

      // act
      var queryBuilder = new QueryBuilder<TestModel>()
         .AddMap("prop1", (field, key) => field.Prop1[key])
         .AddCondition("prop1[key2] = John");
      var actual = queryBuilder.Build(ds);

      // assert
      Assert.Equal(expected.ToString(), actual.ToString());
      Assert.Empty(expected.ToList());
      Assert.Equal(expected.ToList().Count, actual.ToList().Count);
   }

   [Fact]
   private void Filtering_WithDictionaryIntSubKey_ShouldReturnCorrectResult()
   {
      // arrange
      var ds = new List<TestModel>() { new() { Prop2 = new Dictionary<int, string> { { 5987, "John" } } } }.AsQueryable();
      var expected = ds.Where(field => field.Prop2.ContainsKey(5987) && field.Prop2[5987] == "John");

      // act
      var queryBuilder = new QueryBuilder<TestModel>()
         .AddMap("prop2", (field, key) => field.Prop2[key])
         .AddCondition("prop2[5987] = John");
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
         .AddCondition($"prop3[{id}] = John");
      var actual = queryBuilder.Build(ds);

      // assert
      Assert.NotEmpty(expected.ToList());
      Assert.Equal(expected.ToList().Count, actual.ToList().Count);
      Assert.Equal(expected.ToString(), actual.ToString());
   }

   [Fact]
   private void Filtering_WithDictionaryNullableSubKeyValue_ShouldReturnTheCorrectResult()
   {
      // arrange
      var ds = new List<TestModel>() { new() { Prop5 = new Dictionary<string, bool?> { { "subkey", true } } } }.AsQueryable();
      static Expression<Func<TestModel, bool>> BuildPredicate(bool? value)  // generates: (field => field.Prop5["subkey"] == true) without convert()
      {
         var parameter = Expression.Parameter(typeof(TestModel), "field");
         var property = Expression.Property(parameter, nameof(TestModel.Prop5));
         var method = typeof(IDictionary<string, bool?>).GetProperty("Item")?.GetGetMethod()!;
         var indexer = Expression.Call(property, method, Expression.Constant("subkey"));
         var condition = Expression.Equal(indexer, Expression.Constant(value, typeof(bool?)));

         // ((field.Prop5.ContainsKey(subkey)))
         var containsKeyMethod = typeof(IDictionary<string, bool?>).GetMethod("ContainsKey", [typeof(string)]);
         var keyNullCheck = Expression.Call(property, containsKeyMethod!, Expression.Constant("subkey"));

         var combined = Expression.AndAlso(keyNullCheck, condition);
         return Expression.Lambda<Func<TestModel, bool>>(combined, parameter);
      }
      var expected = ds.Where(BuildPredicate(true));

      // act
      var queryBuilder = new QueryBuilder<TestModel>()
         .AddMap("prop5", (field, key) => field.Prop5[key])
         .AddCondition("prop5[subkey] = true");
      var actual = queryBuilder.Build(ds);

      // assert
      Assert.NotEmpty(expected.ToList());
      Assert.Equal(expected.ToList().Count, actual.ToList().Count);
      Assert.Equal(expected.ToString(), actual.ToString());
   }

   [Fact]
   private void FilteringWithGenericAddMap_WithDictionaryGuidSubKey_ShouldReturnTheCorrectResult()
   {
      // arrange
      var id = Guid.Parse("0f0ee7914ba7496aa13ae5ce4ff058b1");
      var ds = new List<TestModel>() { new() { Prop3 = new Dictionary<Guid, string> { { id, "John" } } } }.AsQueryable();
      static Expression<Func<TestModel, bool>> BuildPredicate(Guid id)
      {
         // (field.Prop3[id] == "John")
         var parameter = Expression.Parameter(typeof(TestModel), "field");
         var property = Expression.Property(parameter, nameof(TestModel.Prop3));
         var method = typeof(IDictionary<Guid, string>).GetProperty("Item")?.GetGetMethod()!;
         var indexer = Expression.Call(property, method, Expression.Constant(id));
         var condition = Expression.Equal(indexer, Expression.Constant("John"));

         // ((field.Prop3.ContainsKey(id)))
         var containsKeyMethod = typeof(IDictionary<Guid, string>).GetMethod("ContainsKey", [typeof(Guid)]);
         var keyNullCheck = Expression.Call(property, containsKeyMethod!, Expression.Constant(id));

         var combined = Expression.AndAlso(keyNullCheck, condition);
         return Expression.Lambda<Func<TestModel, bool>>(combined, parameter);
      }
      var expected = ds.Where(BuildPredicate(id));

      // act
      var queryBuilder = new QueryBuilder<TestModel>()
         .AddMap<Guid>("prop3", (field, key) => field.Prop3[key])
         .AddCondition($"prop3[{id:N}] = John");
      var actual = queryBuilder.Build(ds);

      // assert
      Assert.Equal(expected.ToString(), actual.ToString());
      Assert.NotEmpty(expected.ToList());
      Assert.Equal(expected.ToList().Count, actual.ToList().Count);
   }

   [Fact]
   private void FilteringWithGenericAddMap_WithDictionaryLongSubKey_ShouldReturnTheCorrectResult()
   {
      // arrange
      var ds = new List<TestModel>() { new() { Prop4 = new Dictionary<long, long> { { 44, 2024 } } } }.AsQueryable();
      var expected = ds.Where(field => field.Prop4.ContainsKey(44) && field.Prop4[44] == 2024);

      // act
      var queryBuilder = new QueryBuilder<TestModel>()
         .AddMap<long>("prop4", (field, key) => field.Prop4[key])
         .AddCondition("prop4[44] = 2024");
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
      public IDictionary<long, long> Prop4 { get; set; } = new Dictionary<long, long>();
      public IDictionary<string, bool?> Prop5 { get; set; } = new Dictionary<string, bool?>();
   }
}

