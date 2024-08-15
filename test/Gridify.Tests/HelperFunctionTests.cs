using System.Collections.Generic;
using System.Linq;
using Xunit;
using Xunit.Abstractions;
using Gridify.Reflection;
using System;

namespace Gridify.Tests;

public class HelperFunctionTests
{
   private readonly IEnumerable<Type> _simpleTypes;
   private readonly IEnumerable<Type> _complexTypes;
   private readonly IEnumerable<Type> _simpleCollectionTypes;
   private readonly IEnumerable<Type> _complexCollectionTypes;

   private record TestEntity(int Id, string Name);

   public HelperFunctionTests(ITestOutputHelper testOutputHelper)
   {
      _simpleTypes = new List<Type>
      {
         typeof(int),
         typeof(string),
         typeof(DateTime),
         typeof(Guid),
         typeof(bool),
         typeof(decimal),
         typeof(double),
         typeof(float),
         typeof(long),
         typeof(short),
         typeof(byte),
         typeof(char),
         typeof(uint),
         typeof(ulong),
         typeof(ushort)
      };

      _complexTypes = new List<Type>
      {
         typeof(TestClass),
         typeof(TestEntity)
      };

      var collectionTypes = new List<Type>
      {
         typeof(IList<>),
         typeof(List<>),
         typeof(IEnumerable<>),
         typeof(IQueryable<>),
         typeof(ICollection<>),
         typeof(HashSet<>),
      };

      _simpleCollectionTypes = collectionTypes.SelectMany(ct => _simpleTypes.Select(st => ct.MakeGenericType(st))).Concat(_simpleTypes.Select(st => st.MakeArrayType()));
      _complexCollectionTypes = collectionTypes.SelectMany(ct => _complexTypes.Select(st => ct.MakeGenericType(st))).Concat(_complexTypes.Select(ct => ct.MakeArrayType()));
   }

   [Fact]
   public void ShouldDetectCollection()
   {
      Assert.All(_simpleCollectionTypes, (t) => Assert.True(t.IsCollection(out _)));
      Assert.All(_complexCollectionTypes, (t) => Assert.True(t.IsCollection(out _)));
      Assert.All(_simpleTypes, (t) => Assert.False(t.IsCollection(out _)));
      Assert.All(_complexTypes, (t) => Assert.False(t.IsCollection(out _)));
   }

   [Fact]
   public void ShouldDetectSimpleTypeCollection()
   {
      Assert.All(_simpleCollectionTypes, (t) => Assert.True(t.IsSimpleTypeCollection(out _)));
      Assert.All(_complexCollectionTypes, (t) => Assert.False(t.IsSimpleTypeCollection(out _)));
      Assert.All(_simpleTypes, (t) => Assert.False(t.IsSimpleTypeCollection(out _)));
      Assert.All(_complexTypes, (t) => Assert.False(t.IsSimpleTypeCollection(out _)));
   }

   [Fact]
   public void ShouldDetectComplexTypeCollection()
   {
      Assert.All(_simpleCollectionTypes, (t) => Assert.False(t.IsComplexTypeCollection(out _)));
      Assert.All(_complexCollectionTypes, (t) => Assert.True(t.IsComplexTypeCollection(out _)));
      Assert.All(_simpleTypes, (t) => Assert.False(t.IsComplexTypeCollection(out _)));
      Assert.All(_complexTypes, (t) => Assert.False(t.IsComplexTypeCollection(out _)));
   }

   [Fact]
   public void ShouldDetectSimpleType()
   {
      Assert.All(_simpleCollectionTypes, (t) => Assert.False(t.IsSimpleType()));
      Assert.All(_complexCollectionTypes, (t) => Assert.False(t.IsSimpleType()));
      Assert.All(_simpleTypes, (t) => Assert.True(t.IsSimpleType()));
      Assert.All(_complexTypes, (t) => Assert.False(t.IsSimpleType()));
   }
}
