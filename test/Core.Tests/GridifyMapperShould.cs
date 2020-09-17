using System;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Gridify.Tests
{
   public class GridifyMapperShould
   {
      private IGridifyMapper<TestClass> _sut;
      public GridifyMapperShould() => _sut = new GridifyMapper<TestClass>();

      [Fact]
      public void GenerateMappings()
      {
         _sut.GenerateMappings();

         Assert.Equal(typeof(TestClass).GetProperties().Count(), _sut.GetCurrentMaps().Count());
         Assert.True(_sut.HasMap("Id"));
      }

      [Fact]
      public void CaseSensitivity()
      {
         var sut = new GridifyMapper<TestClass>(true);
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
         _sut.AddMap("name", q => q.Name);
         _sut.AddMap("Id", q => q.Id);
         _sut.RemoveMap(nameof(TestClass.Name));
         Assert.Single(_sut.GetCurrentMaps());
      }

      [Fact]
      public void AddMap_DuplicateKey_ShouldThrowErrorIfOverrideIfExistsIsFalse()
      {
         //arrange
         _sut.AddMap("Test", q => q.Name);
         //act
         Action act = () => _sut.AddMap("test", q => q.Name, overrideIfExists: false);
         //assert
         Exception exception = Assert.Throws<Exception>(act);
         //The thrown exception can be used for even more detailed assertions.
         Assert.Equal("Duplicate Key. the 'test' key already exists", exception.Message);
      }

   }
}