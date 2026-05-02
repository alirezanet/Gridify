using System;
using System.Collections.Generic;
using System.Linq;
using Gridify;
using Gridify.Syntax;
using Xunit;

namespace Gridify.Tests;

public class SelectTokenizerTests
{
   [Fact]
   public void Parse_SingleField_ReturnsOnePath()
   {
      var result = SelectTokenizer.Parse("name");
      Assert.Equal(new[] { "name" }, result);
   }

   [Fact]
   public void Parse_MultipleFields_ReturnsList()
   {
      var result = SelectTokenizer.Parse("name,age,tag");
      Assert.Equal(new[] { "name", "age", "tag" }, result);
   }

   [Fact]
   public void Parse_TrimsWhitespaceAroundEachToken()
   {
      var result = SelectTokenizer.Parse(" name , age ,  tag ");
      Assert.Equal(new[] { "name", "age", "tag" }, result);
   }

   [Fact]
   public void Parse_DottedPath_PreservedIntact()
   {
      var result = SelectTokenizer.Parse("address.city,orders.amount");
      Assert.Equal(new[] { "address.city", "orders.amount" }, result);
   }

   [Fact]
   public void Parse_DuplicateTokens_AreDeduplicated()
   {
      var result = SelectTokenizer.Parse("name,age,name,age");
      Assert.Equal(new[] { "name", "age" }, result);
   }

   [Fact]
   public void Parse_NullOrWhitespace_ReturnsEmpty()
   {
      Assert.Empty(SelectTokenizer.Parse(null));
      Assert.Empty(SelectTokenizer.Parse(""));
      Assert.Empty(SelectTokenizer.Parse("   "));
   }

   [Theory]
   [InlineData(",")]
   [InlineData("name,,age")]
   [InlineData(",name")]
   [InlineData("name,")]
   [InlineData(".")]
   [InlineData(".name")]
   [InlineData("name.")]
   [InlineData("name..age")]
   [InlineData("1name")]
   [InlineData("name space")]
   [InlineData("name-bad")]
   public void Parse_InvalidToken_Throws(string input)
   {
      Assert.Throws<GridifySelectException>(() => SelectTokenizer.Parse(input));
   }
}

public class SelectTypeFactoryTests
{
   [Fact]
   public void Create_FlatShape_EmitsTypeWithRequestedProperties()
   {
      var shape = new Gridify.Builder.SelectShape { SourceType = typeof(TestClass) };
      shape.Children.Add(new Gridify.Builder.SelectNode
      {
         Name = "name",
         Accessor = (System.Linq.Expressions.Expression<System.Func<TestClass, string?>>)(t => t.Name),
         ResultType = typeof(string)
      });
      shape.Children.Add(new Gridify.Builder.SelectNode
      {
         Name = "id",
         Accessor = (System.Linq.Expressions.Expression<System.Func<TestClass, int>>)(t => t.Id),
         ResultType = typeof(int)
      });

      var emitted = Gridify.Builder.SelectTypeFactory.GetOrCreate(shape);

      Assert.NotNull(emitted);
      Assert.NotNull(emitted.GetProperty("name"));
      Assert.Equal(typeof(string), emitted.GetProperty("name")!.PropertyType);
      Assert.NotNull(emitted.GetProperty("id"));
      Assert.Equal(typeof(int), emitted.GetProperty("id")!.PropertyType);
      Assert.NotNull(emitted.GetConstructor(System.Type.EmptyTypes));
      Assert.NotNull(System.Reflection.CustomAttributeExtensions.GetCustomAttribute<System.Runtime.CompilerServices.CompilerGeneratedAttribute>(emitted));
   }

   [Fact]
   public void Create_SameShapeTwice_ReturnsSameType()
   {
      Gridify.Builder.SelectShape MakeShape()
      {
         var s = new Gridify.Builder.SelectShape { SourceType = typeof(TestClass) };
         s.Children.Add(new Gridify.Builder.SelectNode
         {
            Name = "name",
            Accessor = (System.Linq.Expressions.Expression<System.Func<TestClass, string?>>)(t => t.Name),
            ResultType = typeof(string)
         });
         return s;
      }

      var a = Gridify.Builder.SelectTypeFactory.GetOrCreate(MakeShape());
      var b = Gridify.Builder.SelectTypeFactory.GetOrCreate(MakeShape());

      Assert.Same(a, b);
   }

   [Fact]
   public void Create_DifferentShapes_ReturnsDifferentTypes()
   {
      var s1 = new Gridify.Builder.SelectShape { SourceType = typeof(TestClass) };
      s1.Children.Add(new Gridify.Builder.SelectNode
      {
         Name = "name",
         Accessor = (System.Linq.Expressions.Expression<System.Func<TestClass, string?>>)(t => t.Name),
         ResultType = typeof(string)
      });

      var s2 = new Gridify.Builder.SelectShape { SourceType = typeof(TestClass) };
      s2.Children.Add(new Gridify.Builder.SelectNode
      {
         Name = "id",
         Accessor = (System.Linq.Expressions.Expression<System.Func<TestClass, int>>)(t => t.Id),
         ResultType = typeof(int)
      });

      var a = Gridify.Builder.SelectTypeFactory.GetOrCreate(s1);
      var b = Gridify.Builder.SelectTypeFactory.GetOrCreate(s2);

      Assert.NotSame(a, b);
   }

   [Fact]
   public void Create_NestedShape_EmitsNestedType()
   {
      var inner = new Gridify.Builder.SelectShape { SourceType = typeof(TestClass) };
      inner.Children.Add(new Gridify.Builder.SelectNode
      {
         Name = "name",
         Accessor = (System.Linq.Expressions.Expression<System.Func<TestClass, string?>>)(t => t.Name),
         ResultType = typeof(string)
      });

      var root = new Gridify.Builder.SelectShape { SourceType = typeof(TestClass) };
      var innerEmitted = Gridify.Builder.SelectTypeFactory.GetOrCreate(inner);
      root.Children.Add(new Gridify.Builder.SelectNode
      {
         Name = "childClass",
         Accessor = (System.Linq.Expressions.Expression<System.Func<TestClass, TestClass?>>)(t => t.ChildClass),
         ResultType = innerEmitted,
         ChildShape = inner
      });

      var emitted = Gridify.Builder.SelectTypeFactory.GetOrCreate(root);
      var childProp = emitted.GetProperty("childClass");

      Assert.NotNull(childProp);
      Assert.Equal(innerEmitted, childProp!.PropertyType);
   }

   [Fact]
   public void Create_ParallelGetOrCreate_IsThreadSafe()
   {
      Gridify.Builder.SelectShape MakeShape()
      {
         var s = new Gridify.Builder.SelectShape { SourceType = typeof(TestClass) };
         s.Children.Add(new Gridify.Builder.SelectNode
         {
            Name = "name",
            Accessor = (System.Linq.Expressions.Expression<System.Func<TestClass, string?>>)(t => t.Name),
            ResultType = typeof(string)
         });
         s.Children.Add(new Gridify.Builder.SelectNode
         {
            Name = "id",
            Accessor = (System.Linq.Expressions.Expression<System.Func<TestClass, int>>)(t => t.Id),
            ResultType = typeof(int)
         });
         return s;
      }

      var results = new System.Collections.Concurrent.ConcurrentBag<System.Type>();
      System.Threading.Tasks.Parallel.For(0, 32, _ =>
      {
         results.Add(Gridify.Builder.SelectTypeFactory.GetOrCreate(MakeShape()));
      });

      Assert.Single(results.Distinct());
   }
}
