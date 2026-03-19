using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

/// <summary>
/// Tests for field-to-field comparison feature (issues #155 and #153).
/// Syntax: <c>field1 = (field2)</c> where <c>(field2)</c> is a field reference on the RHS.
/// </summary>
public class Issue155Tests
{
   // -------------------------------------------------------------------------------
   // Simple (non-nested) field-to-field comparisons
   // -------------------------------------------------------------------------------

   [Fact]
   public void SimpleFieldToFieldEqual_ShouldFilterCorrectly()
   {
      var items = new List<SimpleItem>
      {
         new(1, "Alice", 1),
         new(2, "Bob", 3),
         new(3, "Charlie", 3),
      }.AsQueryable();

      var expected = items.Where(x => x.Id == x.Score);

      var mapper = new GridifyMapper<SimpleItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("id", p => p.Id)
         .AddMap("score", p => p.Score);

      var actual = items.ApplyFiltering("id=(score)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void SimpleFieldToFieldNotEqual_ShouldFilterCorrectly()
   {
      var items = new List<SimpleItem>
      {
         new(1, "Alice", 1),
         new(2, "Bob", 3),
         new(3, "Charlie", 3),
      }.AsQueryable();

      var expected = items.Where(x => x.Id != x.Score);

      var mapper = new GridifyMapper<SimpleItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("id", p => p.Id)
         .AddMap("score", p => p.Score);

      var actual = items.ApplyFiltering("id!=(score)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void SimpleFieldToFieldGreaterThan_ShouldFilterCorrectly()
   {
      var items = new List<SimpleItem>
      {
         new(1, "Alice", 2),
         new(5, "Bob", 3),
         new(3, "Charlie", 3),
      }.AsQueryable();

      var expected = items.Where(x => x.Id > x.Score);

      var mapper = new GridifyMapper<SimpleItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("id", p => p.Id)
         .AddMap("score", p => p.Score);

      var actual = items.ApplyFiltering("id>(score)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void SimpleFieldToFieldLessThan_ShouldFilterCorrectly()
   {
      var items = new List<SimpleItem>
      {
         new(1, "Alice", 2),
         new(5, "Bob", 3),
         new(3, "Charlie", 3),
      }.AsQueryable();

      var expected = items.Where(x => x.Id < x.Score);

      var mapper = new GridifyMapper<SimpleItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("id", p => p.Id)
         .AddMap("score", p => p.Score);

      var actual = items.ApplyFiltering("id<(score)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void SimpleFieldToFieldGreaterOrEqual_ShouldFilterCorrectly()
   {
      var items = new List<SimpleItem>
      {
         new(1, "Alice", 2),
         new(5, "Bob", 3),
         new(3, "Charlie", 3),
      }.AsQueryable();

      var expected = items.Where(x => x.Id >= x.Score);

      var mapper = new GridifyMapper<SimpleItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("id", p => p.Id)
         .AddMap("score", p => p.Score);

      var actual = items.ApplyFiltering("id>=(score)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void SimpleFieldToFieldLessOrEqual_ShouldFilterCorrectly()
   {
      var items = new List<SimpleItem>
      {
         new(1, "Alice", 2),
         new(5, "Bob", 3),
         new(3, "Charlie", 3),
      }.AsQueryable();

      var expected = items.Where(x => x.Id <= x.Score);

      var mapper = new GridifyMapper<SimpleItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("id", p => p.Id)
         .AddMap("score", p => p.Score);

      var actual = items.ApplyFiltering("id<=(score)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   // -------------------------------------------------------------------------------
   // Nested collection field-to-field comparisons (PR #182 original test case)
   // -------------------------------------------------------------------------------

   [Fact]
   public void NestedCollectionFieldToField_LessThan_ShouldReturnCorrectResult()
   {
      List<Item> items =
      [
         new Item("Item1", [new TimeSchedule(1, 2), new TimeSchedule(2, 3)]),
         new Item("Item2", [new TimeSchedule(1, 4), new TimeSchedule(4, 3)]),
         new Item("Item3", [new TimeSchedule(3, 2), new TimeSchedule(2, 3)]),
      ];

      var expected = items.AsQueryable().Where(x => x.Schedules.Any(s => s.End < s.Start));

      var mapper = new GridifyMapper<Item>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("start", p => p.Schedules.Select(x => x.Start))
         .AddMap("end", p => p.Schedules.Select(x => x.End));

      var actual = items.AsQueryable().ApplyFiltering("end<(start)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void NestedCollectionFieldToField_GreaterThan_ShouldReturnCorrectResult()
   {
      List<Item> items =
      [
         new Item("Item1", [new TimeSchedule(1, 2), new TimeSchedule(2, 3)]),
         new Item("Item2", [new TimeSchedule(5, 4), new TimeSchedule(4, 3)]),
         new Item("Item3", [new TimeSchedule(3, 2), new TimeSchedule(2, 3)]),
      ];

      var expected = items.AsQueryable().Where(x => x.Schedules.Any(s => s.End > s.Start));

      var mapper = new GridifyMapper<Item>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("start", p => p.Schedules.Select(x => x.Start))
         .AddMap("end", p => p.Schedules.Select(x => x.End));

      var actual = items.AsQueryable().ApplyFiltering("end>(start)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void NestedCollectionFieldToField_Equal_ShouldReturnCorrectResult()
   {
      List<Item> items =
      [
         new Item("Item1", [new TimeSchedule(2, 2), new TimeSchedule(3, 1)]),
         new Item("Item2", [new TimeSchedule(1, 4), new TimeSchedule(4, 3)]),
         new Item("Item3", [new TimeSchedule(3, 2), new TimeSchedule(2, 2)]),
      ];

      var expected = items.AsQueryable().Where(x => x.Schedules.Any(s => s.End == s.Start));

      var mapper = new GridifyMapper<Item>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("start", p => p.Schedules.Select(x => x.Start))
         .AddMap("end", p => p.Schedules.Select(x => x.End));

      var actual = items.AsQueryable().ApplyFiltering("end=(start)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   // -------------------------------------------------------------------------------
   // Configuration: feature is disabled by default
   // -------------------------------------------------------------------------------

   [Fact]
   public void WhenFeatureDisabled_ShouldThrowGridifyFilteringException()
   {
      var items = new List<SimpleItem> { new(1, "Alice", 1) }.AsQueryable();

      // AllowFieldToFieldComparison defaults to false
      var mapper = new GridifyMapper<SimpleItem>()
         .AddMap("id", p => p.Id)
         .AddMap("score", p => p.Score);

      Assert.Throws<GridifyFilteringException>(() => items.ApplyFiltering("id=(score)", mapper));
   }

   [Fact]
   public void WhenGlobalConfigEnabled_ShouldWork()
   {
      var original = GridifyGlobalConfiguration.AllowFieldToFieldComparison;
      try
      {
         GridifyGlobalConfiguration.AllowFieldToFieldComparison = true;

         var items = new List<SimpleItem>
         {
            new(1, "Alice", 1),
            new(2, "Bob", 3),
         }.AsQueryable();

         var mapper = new GridifyMapper<SimpleItem>()
            .AddMap("id", p => p.Id)
            .AddMap("score", p => p.Score);

         var actual = items.ApplyFiltering("id=(score)", mapper);
         Assert.Single(actual);
         Assert.Equal(1, actual.First().Id);
      }
      finally
      {
         GridifyGlobalConfiguration.AllowFieldToFieldComparison = original;
      }
   }

   // -------------------------------------------------------------------------------
   // QueryBuilder integration
   // -------------------------------------------------------------------------------

   [Fact]
   public void QueryBuilder_FieldToField_ShouldWork()
   {
      var items = new List<SimpleItem>
      {
         new(1, "Alice", 1),
         new(2, "Bob", 3),
         new(3, "Charlie", 3),
      }.AsQueryable();

      var expected = items.Where(x => x.Id == x.Score);

      var mapper = new GridifyMapper<SimpleItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("id", p => p.Id)
         .AddMap("score", p => p.Score);

      var actual = new QueryBuilder<SimpleItem>()
         .UseCustomMapper(mapper)
         .AddCondition("id=(score)")
         .Build(items);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void QueryBuilder_FieldToField_NestedCollection_ShouldWork()
   {
      List<Item> items =
      [
         new Item("Item1", [new TimeSchedule(1, 2), new TimeSchedule(2, 3)]),
         new Item("Item2", [new TimeSchedule(1, 4), new TimeSchedule(4, 3)]),
         new Item("Item3", [new TimeSchedule(3, 2), new TimeSchedule(2, 3)]),
      ];

      var expected = items.AsQueryable().Where(x => x.Schedules.Any(s => s.End < s.Start));

      var mapper = new GridifyMapper<Item>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("start", p => p.Schedules.Select(x => x.Start))
         .AddMap("end", p => p.Schedules.Select(x => x.End));

      var actual = new QueryBuilder<Item>()
         .UseCustomMapper(mapper)
         .AddCondition("end<(start)")
         .Build(items.AsQueryable());

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   // -------------------------------------------------------------------------------
   // Combined with other conditions
   // -------------------------------------------------------------------------------

   [Fact]
   public void FieldToField_CombinedWithValueFilter_ShouldWork()
   {
      var items = new List<SimpleItem>
      {
         new(1, "Alice", 1),
         new(2, "Bob", 3),
         new(3, "Charlie", 3),
         new(4, "Dave", 4),
      }.AsQueryable();

      // id == score AND id > 1
      var expected = items.Where(x => x.Id == x.Score && x.Id > 1);

      var mapper = new GridifyMapper<SimpleItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("id", p => p.Id)
         .AddMap("score", p => p.Score);

      var actual = items.ApplyFiltering("id=(score),id>1", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   // -------------------------------------------------------------------------------
   // String operators for field-to-field comparison
   // -------------------------------------------------------------------------------

   [Fact]
   public void FieldToField_Like_ShouldFilterCorrectly()
   {
      var items = new List<StringItem>
      {
         new("Hello World", "World"),
         new("Hello World", "Missing"),
         new("Gridify", "Grid"),
      }.AsQueryable();

      var expected = items.Where(x => x.Name.Contains(x.Tag));

      var mapper = new GridifyMapper<StringItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("name", p => p.Name)
         .AddMap("tag", p => p.Tag);

      var actual = items.ApplyFiltering("name=*(tag)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void FieldToField_NotLike_ShouldFilterCorrectly()
   {
      var items = new List<StringItem>
      {
         new("Hello World", "World"),
         new("Hello World", "Missing"),
         new("Gridify", "Grid"),
      }.AsQueryable();

      var expected = items.Where(x => !x.Name.Contains(x.Tag));

      var mapper = new GridifyMapper<StringItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("name", p => p.Name)
         .AddMap("tag", p => p.Tag);

      var actual = items.ApplyFiltering("name!*(tag)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void FieldToField_StartsWith_ShouldFilterCorrectly()
   {
      var items = new List<StringItem>
      {
         new("Hello World", "Hello"),
         new("Hello World", "World"),
         new("Gridify", "Grid"),
      }.AsQueryable();

      var expected = items.Where(x => x.Name.StartsWith(x.Tag));

      var mapper = new GridifyMapper<StringItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("name", p => p.Name)
         .AddMap("tag", p => p.Tag);

      var actual = items.ApplyFiltering("name^(tag)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void FieldToField_NotStartsWith_ShouldFilterCorrectly()
   {
      var items = new List<StringItem>
      {
         new("Hello World", "Hello"),
         new("Hello World", "World"),
         new("Gridify", "Grid"),
      }.AsQueryable();

      var expected = items.Where(x => !x.Name.StartsWith(x.Tag));

      var mapper = new GridifyMapper<StringItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("name", p => p.Name)
         .AddMap("tag", p => p.Tag);

      var actual = items.ApplyFiltering("name!^(tag)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void FieldToField_EndsWith_ShouldFilterCorrectly()
   {
      var items = new List<StringItem>
      {
         new("Hello World", "World"),
         new("Hello World", "Hello"),
         new("Gridify", "ify"),
      }.AsQueryable();

      var expected = items.Where(x => x.Name.EndsWith(x.Tag));

      var mapper = new GridifyMapper<StringItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("name", p => p.Name)
         .AddMap("tag", p => p.Tag);

      var actual = items.ApplyFiltering("name$(tag)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void FieldToField_NotEndsWith_ShouldFilterCorrectly()
   {
      var items = new List<StringItem>
      {
         new("Hello World", "World"),
         new("Hello World", "Hello"),
         new("Gridify", "ify"),
      }.AsQueryable();

      var expected = items.Where(x => !x.Name.EndsWith(x.Tag));

      var mapper = new GridifyMapper<StringItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("name", p => p.Name)
         .AddMap("tag", p => p.Tag);

      var actual = items.ApplyFiltering("name!$(tag)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   // -------------------------------------------------------------------------------
   // Grouping/parentheses conflict tests - ensure field reference (fieldName) on RHS
   // does NOT conflict with Gridify's existing grouping syntax  ( expr | expr )
   // -------------------------------------------------------------------------------

   [Fact]
   public void FieldToField_WithGroupingOnLeft_ShouldWork()
   {
      // "(name=*J|name=*S)" is a group expression - should remain a group
      // "id=(score)" is a field-to-field comparison - should be a field reference
      var items = new List<SimpleItem>
      {
         new(1, "John", 1),
         new(2, "Sara", 3),
         new(3, "Bob", 3),
         new(4, "Dave", 4),
      }.AsQueryable();

      // (name contains J OR name contains S) AND id == score
      var expected = items.Where(x => (x.Name.Contains("J") || x.Name.Contains("S")) && x.Id == x.Score);

      var mapper = new GridifyMapper<SimpleItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("id", p => p.Id)
         .AddMap("score", p => p.Score)
         .AddMap("name", p => p.Name);

      var actual = items.ApplyFiltering("(name=*J|name=*S),id=(score)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void FieldToField_InsideGrouping_ShouldWork()
   {
      // field-to-field comparison inside a group: (id=(score)|id>2)
      var items = new List<SimpleItem>
      {
         new(1, "Alice", 1),
         new(2, "Bob", 3),
         new(3, "Charlie", 3),
         new(5, "Dave", 4),
      }.AsQueryable();

      // id == score OR id > 2
      var expected = items.Where(x => x.Id == x.Score || x.Id > 2);

      var mapper = new GridifyMapper<SimpleItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("id", p => p.Id)
         .AddMap("score", p => p.Score);

      var actual = items.ApplyFiltering("(id=(score)|id>2)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void GroupingWithoutFieldReference_ShouldNotBeAffectedByFeature()
   {
      // Ensure regular grouping still works when feature is DISABLED
      var items = new List<SimpleItem>
      {
         new(1, "John", 1),
         new(3, "Sara", 3),
         new(4, "Bob", 4),
      }.AsQueryable();

      var expected = items.Where(x => (x.Name.Contains("J") || x.Name.Contains("S")) && x.Id < 5);

      // Feature disabled, but grouping should still work normally
      var mapper = new GridifyMapper<SimpleItem>()
         .AddMap("id", p => p.Id)
         .AddMap("name", p => p.Name);

      var actual = items.ApplyFiltering("(name=*J|name=*S),id<5", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void ParenthesisInValue_ShouldStillBeEscapedAsValue()
   {
      // Escaped parentheses in value should still be treated as a literal value, not a field reference
      var items = new List<SimpleItem>
      {
         new(1, "test(value)", 1),
         new(2, "normal", 2),
      }.AsQueryable();

      var expected = items.Where(x => x.Name == "test(value)");

      // feature enabled, but escaped '(' should still be a value
      var mapper = new GridifyMapper<SimpleItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("name", p => p.Name);

      // The value contains escaped parentheses \( and \) which should be treated as literal characters
      var actual = items.ApplyFiltering(@"name=test\(value\)", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void FieldToField_ORWithValueFilter_ShouldWork()
   {
      var items = new List<SimpleItem>
      {
         new(1, "Alice", 1),
         new(2, "Bob", 3),
         new(3, "Charlie", 5),
         new(5, "Dave", 5),
      }.AsQueryable();

      // id == score OR id == 2
      var expected = items.Where(x => x.Id == x.Score || x.Id == 2);

      var mapper = new GridifyMapper<SimpleItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("id", p => p.Id)
         .AddMap("score", p => p.Score);

      var actual = items.ApplyFiltering("id=(score)|id=2", mapper);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   // -------------------------------------------------------------------------------
   // QueryBuilder integration - advanced scenarios
   // -------------------------------------------------------------------------------

   [Fact]
   public void QueryBuilder_FieldToField_WithBuildFilteringExpression_ShouldWork()
   {
      var items = new List<SimpleItem>
      {
         new(1, "Alice", 1),
         new(2, "Bob", 3),
         new(3, "Charlie", 3),
      }.AsQueryable();

      var expected = items.Where(x => x.Id == x.Score);

      var mapper = new GridifyMapper<SimpleItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("id", p => p.Id)
         .AddMap("score", p => p.Score);

      var filter = new GridifyQuery { Filter = "id=(score)" };
      var expr = filter.GetFilteringExpression(mapper);
      var actual = items.Where(expr);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   [Fact]
   public void QueryBuilder_FieldToField_ComplexConditions_ShouldWork()
   {
      var items = new List<SimpleItem>
      {
         new(1, "Alice", 1),
         new(2, "Bob", 3),
         new(3, "Charlie", 3),
         new(4, "Dave", 4),
         new(5, "Eve", 3),
      }.AsQueryable();

      // (id == score OR id > 4) AND id >= 1
      var expected = items.Where(x => (x.Id == x.Score || x.Id > 4) && x.Id >= 1);

      var mapper = new GridifyMapper<SimpleItem>(new GridifyMapperConfiguration { AllowFieldToFieldComparison = true })
         .AddMap("id", p => p.Id)
         .AddMap("score", p => p.Score);

      var actual = new QueryBuilder<SimpleItem>()
         .UseCustomMapper(mapper)
         .AddCondition("(id=(score)|id>4),id>=1")
         .Build(items);

      Assert.Equal(expected.ToList(), actual.ToList());
   }

   // -------------------------------------------------------------------------------
   // Model types used in tests
   // -------------------------------------------------------------------------------

   private record SimpleItem(int Id, string Name, int Score);

   private record StringItem(string Name, string Tag);

   private record Item(string Name, List<TimeSchedule> Schedules);

   private record TimeSchedule(int Start, int End);
}
