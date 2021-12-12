using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests;

public class GridifyNestedCollectionTests
{
   private readonly List<Level1> _fakeRepository3Nesting;
   private readonly List<Level2> _fakeRepository2Nesting;

   public GridifyNestedCollectionTests()
   {
      _fakeRepository3Nesting = new List<Level1>(GetSampleDataWith3Nesting());
      _fakeRepository2Nesting = new List<Level2>(GetSampleDataWith2Nestings());
   }


   [Fact]
   public void Filtering_OnThirdLevelNestedProperty()
   {
      var gm = new GridifyMapper<Level1>()
         .GenerateMappings()
         .AddMap("prop1", l1 => l1.Level2List.SelectMany(l2 => l2.Level3List).Select(l3 => l3.Property1));


      var actual = _fakeRepository3Nesting.AsQueryable()
         .ApplyFiltering("prop1 <= 3", gm)
         .ToList();

      var expected = _fakeRepository3Nesting.Where(q => q.Level2List.Any(w => w.Level3List.Any(z => z.Property1 <= 3))).ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact] //https://github.com/alirezanet/Gridify/issues/17 #17
   public void Filtering_OnThirdLevelNestedPropertyWithMultipleChainedConditions()
   {
      var gm = new GridifyMapper<Level1>()
         .GenerateMappings()
         .AddMap("Level2List_Level3List_Property1", l1 => l1.Level2List.SelectMany(l2 => l2.Level3List).Select(l3 => l3.Property1))
         .AddMap("Level2List_Id", l1 => l1.Level2List.Select(l2 => l2.Id));

      var actual = _fakeRepository3Nesting.AsQueryable()
         .ApplyFiltering("(Level2List_Id = 101, Level2List_Level3List_Property1 >= 3) , id < 10", gm)
         .ToList();


      var expected = _fakeRepository3Nesting.AsQueryable().Where(
         l1 => l1.Level2List != null &&
               l1.Level2List.Any(l2 => l2.Id == 101 &&
                                       l2.Level3List != null &&
                                       l2.Level3List.Any(l3 => l3.Property1 >= 3))).ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.False(actual.Any());
   }

   [Fact] //https://github.com/alirezanet/Gridify/issues/17 #17
   public void Filtering_OnThirdLevelNestedPropertyWithMultipleUnChainedConditions()
   {
      var gm = new GridifyMapper<Level1>()
         .GenerateMappings()
         .AddMap("Level2List_Level3List_Property1", l1 => l1.Level2List.SelectMany(l2 => l2.Level3List).Select(l3 => l3.Property1))
         .AddMap("Level2List_Id", l1 => l1.Level2List.Select(l2 => l2.Id));

      var actual = _fakeRepository3Nesting.AsQueryable()
         .ApplyFiltering("Level2List_Id = 101, Level2List_Level3List_Property1 >= 3,id < 10", gm)
         .ToList();


      var expected = _fakeRepository3Nesting.AsQueryable().Where(
         l1 => l1.Level2List != null && l1.Level2List.Any(l2 => l2.Id == 101) &&
               l1.Level2List.Any(l2 => l2.Level3List.Any(l3 => l3.Property1 >= 3))).ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void Filtering_OnThirdLevelNestedPropertyWithMultipleUnChainedConditionsWithNestedParenthesis()
   {
      var gm = new GridifyMapper<Level1>()
         .GenerateMappings()
         .AddMap("Level2List_Level3List_Property1", l1 => l1.Level2List.SelectMany(l2 => l2.Level3List).Select(l3 => l3.Property1))
         .AddMap("Level2List_Id", l1 => l1.Level2List.Select(l2 => l2.Id));

      var actual = _fakeRepository3Nesting.AsQueryable()
         .ApplyFiltering("( (id < 10 ), Level2List_Id = 101, Level2List_Level3List_Property1 >= 3)", gm)
         .ToList();


      var expected = _fakeRepository3Nesting.AsQueryable().Where(
         l1 => l1.Level2List != null && l1.Level2List.Any(l2 => l2.Id == 101) &&
               l1.Level2List.Any(l2 => l2.Level3List.Any(l3 => l3.Property1 >= 3))).ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void Filtering_OnSecondLevelNestedProperty()
   {
      var gm = new GridifyMapper<Level2>()
         .GenerateMappings()
         .AddMap("prop1", l2 => l2.Level3List.Select(l3 => l3.Property1));


      var actual = _fakeRepository2Nesting.AsQueryable()
         .ApplyFiltering("prop1 <= 3", gm)
         .ToList();

      var expected = _fakeRepository2Nesting.Where(q => q.Level3List.Any(z => z.Property1 <= 3)).ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void Filtering_OnThirdLevelNestedPropertyUsingSecondLevelProp()
   {
      var gm = new GridifyMapper<Level1>()
         .GenerateMappings()
         .AddMap("lvl", l1 => l1.Level2List.Select(l2 => l2.ChildProp).SelectMany(sl2 => sl2.Level3List).Select(l3 => l3.Level));

      var actual = _fakeRepository3Nesting.AsQueryable()
         .ApplyFiltering("lvl < 2", gm)
         .ToList();

      var expected = _fakeRepository3Nesting.Where(l1 => l1.Level2List != null && l1.Level2List.Any(l2 => l2.ChildProp != null &&
         l2.ChildProp.Level3List != null &&
         l2.ChildProp.Level3List.Any(l3 => l3.Level < 2))).ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }


   [Fact] // issue #29 https://github.com/alirezanet/Gridify/issues/29 
   public void ApplyFiltering_UsingSubCollection_PassIndexes()
   {
      var gm = new GridifyMapper<Level2>()
         .GenerateMappings()
         .AddMap("l3p2", (l1, index) => l1.Level3List[index].Property2);
         
      var expected = _fakeRepository2Nesting.Where(q => q.Level3List[0].Property2 > 50).ToList();
      var actual = _fakeRepository2Nesting.AsQueryable().ApplyFiltering("l3p2[1] > 50", gm).ToList();
         
      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }
    
   #region TestData

   private IEnumerable<Level1> GetSampleDataWith3Nesting()
   {
      var subLvl2 = new ChildProp()
      {
         Level3List = new List<Level3>() { new Level3() { Property1 = 2.0, Property2 = 100.0, Level = 1 } }
      };
      var subLvl22 = new ChildProp()
      {
         Level3List = new List<Level3>() { new Level3() { Property1 = 3.0, Property2 = 100.0, Level = 3 } }
      };

      yield return new Level1()
      {
         Id = 1,
         Name = "Level1Name",
         Level2List = new List<Level2>()
         {
            new Level2()
            {
               Id = 101, Name = "Level2_1", Level3List = new List<Level3>() { new Level3() { Property1 = 2.0, Property2 = 100.0, Level = 0 } }
            },
            new Level2()
            {
               Id = 102, Name = "Level2_2", Level3List = new List<Level3>() { new Level3() { Property1 = 3.0, Property2 = 200.0, Level = 1 } }
            },
            new Level2()
            {
               Id = 103, Name = "Level2_3", Level3List = new List<Level3>() { new Level3() { Property1 = 4.0, Property2 = 300.0, Level = 2 } }
            }
         }
      };

      yield return new Level1()
      {
         Id = 1,
         Name = "Level1Name",
         Level2List = new List<Level2>()
         {
            new Level2()
            {
               Id = 101, Name = "Level2_1", ChildProp = subLvl2,
               Level3List = new List<Level3>() { new Level3() { Property1 = 2.0, Property2 = 100.0, Level = 0 } }
            },
            new Level2()
            {
               Id = 102, Name = "Level2_2", ChildProp = new ChildProp(),
               Level3List = new List<Level3>() { new Level3() { Property1 = 3.0, Property2 = 200.0, Level = 0 } }
            },
            new Level2()
            {
               Id = 103, Name = "Level2_3", Level3List = new List<Level3>() { new Level3() { Property1 = 4.0, Property2 = 300.0, Level = 0 } }
            }
         }
      };
      yield return new Level1()
      {
         Id = 2,
         Name = "Level1Name2",
         Level2List = new List<Level2>()
         {
            new Level2()
            {
               Id = 108, Name = "Level2_1", Level3List = new List<Level3>() { new Level3() { Property1 = 4.0, Property2 = 100.0, Level = 0 } }
            },
            new Level2()
            {
               Id = 109, Name = "Level2_2", ChildProp = new ChildProp(),
               Level3List = new List<Level3>() { new Level3() { Property1 = 5.0, Property2 = 200.0, Level = 0 } }
            },
            new Level2()
            {
               Id = 110, Name = "Level2_3", ChildProp = subLvl22,
               Level3List = new List<Level3>() { new Level3() { Property1 = 6.0, Property2 = 300.0, Level = 0 } }
            }
         }
      };
      yield return new Level1()
      {
         Id = 3,
         Name = "Level1Name3",
         Level2List = new List<Level2>()
         {
            new Level2()
            {
               Id = 111, Name = "Level2_1", Level3List = new List<Level3>() { new Level3() { Property1 = 6.0, Property2 = 100.0, Level = 0 } }
            },
            new Level2()
            {
               Id = 112, Name = "Level2_2", Level3List = new List<Level3>() { new Level3() { Property1 = 7.0, Property2 = 200.0, Level = 0 } }
            },
            new Level2()
            {
               Id = 113, Name = "Level2_3", Level3List = new List<Level3>() { new Level3() { Property1 = 8.0, Property2 = 300.0, Level = 0 } }
            }
         }
      };
   }

   private IEnumerable<Level2> GetSampleDataWith2Nestings()
   {
      yield return new Level2()
      {
         Id = 1,
         Name = "Level2Name",
         Level3List = new List<Level3>()
         {
            new Level3() { Property1 = 2.0, Property2 = 100.0, Level = 0 },
            new Level3() { Property1 = 3.0, Property2 = 100.0, Level = 0 },
            new Level3() { Property1 = 4.0, Property2 = 100.0, Level = 0 }
         }
      };
      yield return new Level2()
      {
         Id = 2,
         Name = "Level2Name2",
         Level3List = new List<Level3>()
         {
            new Level3() { Property1 = 4.0, Property2 = 100.0, Level = 0 },
            new Level3() { Property1 = 5.0, Property2 = 100.0, Level = 0 },
            new Level3() { Property1 = 6.0, Property2 = 100.0, Level = 0 }
         }
      };
   }
#nullable disable
   public class Level1
   {
      public int Id { get; set; }
      public string Name { get; set; }
      public List<Level2> Level2List { get; set; }
   }

   public class Level2
   {
      public int Id { get; set; }
      public string Name { get; set; }
      public List<Level3> Level3List { get; set; }

      public ChildProp ChildProp { get; set; }
   }

   public class ChildProp
   {
      public List<Level3> Level3List { get; set; }
   }

   public class Level3
   {
      public int Level { get; set; }
      public double Property1 { get; set; }
      public double Property2 { get; set; }
   }

   #endregion
}