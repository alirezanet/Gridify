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
   // Model types used in tests
   // -------------------------------------------------------------------------------

   private record SimpleItem(int Id, string Name, int Score);

   private record Item(string Name, List<TimeSchedule> Schedules);

   private record TimeSchedule(int Start, int End);
}
