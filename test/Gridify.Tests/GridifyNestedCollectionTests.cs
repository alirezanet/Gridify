using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests
{
   public class GridifyNestedCollectionTests
   {
      private readonly List<Level1> _fakeRepository3Nesting;
      private readonly List<Level2> _fakeRepository2Nesting;

      public GridifyNestedCollectionTests()
      {
         _fakeRepository3Nesting = new List<Level1>(GetSampleDataWith3Nestings());
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


      #region TestData

      private IEnumerable<Level1> GetSampleDataWith3Nestings()
      {
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
                  Id = 102, Name = "Level2_2", Level3List = new List<Level3>() { new Level3() { Property1 = 3.0, Property2 = 200.0, Level = 0 } }
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
                  Id = 101, Name = "Level2_1", Level3List = new List<Level3>() { new Level3() { Property1 = 4.0, Property2 = 100.0, Level = 0 } }
               },
               new Level2()
               {
                  Id = 102, Name = "Level2_2", Level3List = new List<Level3>() { new Level3() { Property1 = 5.0, Property2 = 200.0, Level = 0 } }
               },
               new Level2()
               {
                  Id = 103, Name = "Level2_3", Level3List = new List<Level3>() { new Level3() { Property1 = 6.0, Property2 = 300.0, Level = 0 } }
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
                  Id = 101, Name = "Level2_1", Level3List = new List<Level3>() { new Level3() { Property1 = 6.0, Property2 = 100.0, Level = 0 } }
               },
               new Level2()
               {
                  Id = 102, Name = "Level2_2", Level3List = new List<Level3>() { new Level3() { Property1 = 7.0, Property2 = 200.0, Level = 0 } }
               },
               new Level2()
               {
                  Id = 103, Name = "Level2_3", Level3List = new List<Level3>() { new Level3() { Property1 = 8.0, Property2 = 300.0, Level = 0 } }
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
      }

      public class Level3
      {
         public int Level { get; set; }
         public double Property1 { get; set; }
         public double Property2 { get; set; }
      }

      #endregion
   }
}