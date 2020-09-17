using System;
using System.Linq;
using System.Linq.Expressions;
using Xunit;

namespace Gridify.Tests
{
   public class GridifyMapperShould
   {
      private IGridifyMapper<TestClass> _sut;
      public GridifyMapperShould () => _sut = new GridifyMapper<TestClass> ();     

      [Fact]
      public void GenerateMappings ()
      {
         _sut.GenerateMappings ();

         Assert.Equal (typeof (TestClass).GetProperties ().Count (), _sut.GetCurrentMaps().Count());
         Assert.True (_sut.HasMap ("Id"));
      }

      [Fact]
      public void CaseSensitivity ()
      {
         var sut = new GridifyMapper<TestClass> (true);
         sut.AddMap ("id", q => q.Id );

         Assert.True (sut.HasMap ("id"));
         Assert.False (sut.HasMap ("ID"));
      }

      [Fact]
      public void AddMap ()
      {
         _sut.AddMap (nameof (TestClass.Name), p => p.Name);
         Assert.Single (_sut.GetCurrentMaps());
      }

      [Fact]
      public void RemoveMap ()
      {
         _sut.AddMap ("name", q => q.Name);
         _sut.AddMap ("Id", q => q.Id);
         _sut.RemoveMap (nameof (TestClass.Name));
         Assert.Single (_sut.GetCurrentMaps());
      }

   }
}