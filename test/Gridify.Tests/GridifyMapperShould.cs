using Gridify.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests;

public class GridifyMapperShould
{
   private IGridifyMapper<TestClass> _sut;
   public GridifyMapperShould() => _sut = new GridifyMapper<TestClass>();

   [Fact]
   public void GenerateMappings()
   {
      _sut.ClearMappings();
      _sut.GenerateMappings();

      var props = typeof(TestClass).GetProperties()
         .Where(q => !q.PropertyType.IsComplexTypeCollection(out _) && (!q.PropertyType.IsClass || q.PropertyType == typeof(string)));

      Assert.Equal(props.Count(), _sut.GetCurrentMaps().Count());
      Assert.True(_sut.HasMap("Id"));
   }

   [Fact]
   public void GenerateNestedMappings()
   {
      _sut.ClearMappings();
      _sut.GenerateMappings(5);

      var maps = _sut.GetCurrentMaps();

      var testClass = new TestClass
      {
         Id = 1,
         ChildClass = new TestClass
         {
            Id = 11,
            Children = [
               new TestClass {
                  Id = 111,
                  ChildClass = new TestClass {
                     Id = 1111
                  }
               }
            ]
         },
         Children = [
            new TestClass {
               Id = 12,
               ChildClass = new TestClass {
                  Id = 121,
                  Children = [
                     new TestClass {
                        Id = 1211,
                        ChildClass = new TestClass {
                           Id = 12111
                        }
                     }
                  ],
               },
               Children = [
                  new TestClass {
                     Id = 122
                  }
               ],
            },
            new TestClass {
               Id = 13,
               ChildClass = new TestClass {
                  Id = 131,
                  Children = [
                     new TestClass {
                        Id = 1311,
                        ChildClass = new TestClass {
                           Id = 13111
                        }
                     }
                  ],
               },
               Children = [
                  new TestClass {
                     Id = 132
                  }
               ]
            }
         ]
      };

      var func = maps.First(m => m.From == "children.childClass.children.childClass.id").To.Compile();
      var result = func.DynamicInvoke(testClass)!;
      Assert.Equivalent(result, new List<int> { 12111, 13111 });

      var func2 = maps.First(m => m.From == "childClass.children.childClass.id").To.Compile();
      var result2 = func2.DynamicInvoke(testClass)!;
      Assert.Equivalent(result2, new List<int> { 1111 });

   }

   [Fact]
   public void CaseSensitivity()
   {
      var sut = new GridifyMapper<TestClass>(q => q.CaseSensitive = true);
      sut.AddMap("id", q => q.Id);

      Assert.True(sut.HasMap("id"));
      Assert.False(sut.HasMap("ID"));
   }

   [Fact]
   public void AddMap()
   {
      _sut.AddMap(nameof(TestClass.Name), p => p.Name);
      Assert.Single(_sut.GetCurrentMaps());
   }

   [Fact]
   public void RemoveMap()
   {
      _sut.Configuration.CaseSensitive = false;
      _sut.AddMap("name", q => q.Name);
      _sut.AddMap("childDate", q => q.ChildClass!.MyDateTime);
      _sut.RemoveMap(nameof(TestClass.Name));
      Assert.Single(_sut.GetCurrentMaps());
   }

   [Fact]
   public void GridifyMapperToStringShouldReturnFieldsList()
   {
      _sut.AddMap("name", q => q.Name);
      _sut.AddMap("childDate", q => q.ChildClass!.MyDateTime);
      var actual = _sut.ToString();

      Assert.Equal("name,childDate", actual);
   }


   [Fact]
   public void AddMap_DuplicateKey_ShouldThrowErrorIfOverrideIfExistsIsFalse()
   {
      //arrange
      _sut.AddMap("Test", q => q.Name);
      //act
      Action act = () => _sut.AddMap("test", q => q.Name, overrideIfExists: false);
      //assert
      var exception = Assert.Throws<GridifyMapperException>(act);
      //The thrown exception can be used for even more detailed assertions.
      Assert.Equal("Duplicate Key. the 'test' key already exists", exception.Message);
   }
   
   [Theory]
   [InlineData(typeof(DateTime), 0)]
   [InlineData(typeof(DateTime?), 1)]
   [InlineData(typeof(Guid), 1)]
   [InlineData(typeof(string), 2)]
   [InlineData(typeof(bool), 1)]
   [InlineData(typeof(int), 1)]
   public void GetMappingByType(Type filterType, int count)
   {
      var gm = new GridifyMapper<TestClass>(true);
      var maps = gm.GetCurrentMapsByType([filterType]);
      Assert.Equal(count, maps.Count());
   }

   [Fact]
   public void GetMappingsByCustomType()
   {
      var gm = new GridifyMapper<TypeMappingTest>().GenerateMappings();
      var maps = gm.GetCurrentMapsByType([typeof(IEnumerable<string>)]);
      Assert.Single(maps);
   }
}

public class TypeMappingTest
{
   public List<string> MyListString { get; set; } = [];
}