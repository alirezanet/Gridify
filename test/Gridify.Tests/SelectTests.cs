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

public class SelectExpressionBuilderTests
{
   [Fact]
   public void Build_FlatProjection_ReturnsLambdaThatProjectsCorrectly()
   {
      var mapper = new GridifyMapper<TestClass>(autoGenerateMappings: true);
      var lambda = Gridify.Builder.SelectExpressionBuilder<TestClass>.Build("name,id", mapper);

      var compiled = lambda.Compile();
      var input = new TestClass(7, "Alice", null);
      var projected = compiled(input);

      var nameProp = projected.GetType().GetProperty("name");
      var idProp = projected.GetType().GetProperty("id");
      Assert.NotNull(nameProp);
      Assert.NotNull(idProp);
      Assert.Equal("Alice", nameProp!.GetValue(projected));
      Assert.Equal(7, idProp!.GetValue(projected));
   }

   [Fact]
   public void Build_NestedPath_ProducesNestedProjection()
   {
      var mapper = new GridifyMapper<TestClass>(autoGenerateMappings: true, maxNestingDepth: 2);
      var lambda = Gridify.Builder.SelectExpressionBuilder<TestClass>.Build("childClass.name", mapper);

      var compiled = lambda.Compile();
      var input = new TestClass(1, "Outer", new TestClass(2, "Inner", null));
      var projected = compiled(input);

      var childProp = projected.GetType().GetProperty("childClass");
      Assert.NotNull(childProp);

      var inner = childProp!.GetValue(projected);
      Assert.NotNull(inner);
      var innerNameProp = inner!.GetType().GetProperty("name");
      Assert.Equal("Inner", innerNameProp!.GetValue(inner));
   }

   [Fact]
   public void Build_CollectionPath_ProducesEnumerableProjection()
   {
      var mapper = new GridifyMapper<TestClass>(autoGenerateMappings: true, maxNestingDepth: 2);
      var lambda = Gridify.Builder.SelectExpressionBuilder<TestClass>.Build("children.name", mapper);

      var compiled = lambda.Compile();
      var input = new TestClass(1, "Parent", null);
      input.Children.Add(new TestClass(2, "ChildA", null));
      input.Children.Add(new TestClass(3, "ChildB", null));

      var projected = compiled(input);
      var childrenProp = projected.GetType().GetProperty("children");
      Assert.NotNull(childrenProp);

      var children = childrenProp!.GetValue(projected) as System.Collections.IEnumerable;
      Assert.NotNull(children);

      var names = new List<string?>();
      foreach (var c in children!)
      {
         var nameProp = c.GetType().GetProperty("name");
         names.Add((string?)nameProp!.GetValue(c));
      }
      Assert.Equal(new[] { "ChildA", "ChildB" }, names);
   }

   [Fact]
   public void Build_UnmappedField_ThrowsByDefault()
   {
      var mapper = new GridifyMapper<TestClass>().AddMap("name", x => x.Name!);
      Assert.Throws<GridifySelectException>(() =>
         Gridify.Builder.SelectExpressionBuilder<TestClass>.Build("name,missingField", mapper));
   }

   [Fact]
   public void Build_UnmappedField_WhenIgnoreNotMappedFields_DropsSilently()
   {
      var mapper = new GridifyMapper<TestClass>(c => c.IgnoreNotMappedFields = true)
         .AddMap("name", x => x.Name!);

      var lambda = Gridify.Builder.SelectExpressionBuilder<TestClass>.Build("name,missingField", mapper);
      var compiled = lambda.Compile();
      var projected = compiled(new TestClass(1, "Bob", null));

      Assert.NotNull(projected.GetType().GetProperty("name"));
      Assert.Null(projected.GetType().GetProperty("missingField"));
   }

   [Fact]
   public void Build_TwoLevelCollection_Throws()
   {
      var mapper = new GridifyMapper<TestClass>(autoGenerateMappings: true, maxNestingDepth: 3);
      Assert.Throws<GridifySelectException>(() =>
         Gridify.Builder.SelectExpressionBuilder<TestClass>.Build("children.children.name", mapper));
   }

   [Fact]
   public void Build_EmptySelect_Throws()
   {
      var mapper = new GridifyMapper<TestClass>(autoGenerateMappings: true);
      Assert.Throws<GridifySelectException>(() =>
         Gridify.Builder.SelectExpressionBuilder<TestClass>.Build("", mapper));
   }

   [Fact]
   public void Build_MixedCaseFirstSegment_GroupsByMapperCaseSensitivity()
   {
      // Default mapper is case-insensitive. "Name" and "name" should be one property,
      // not two, otherwise the emitted type has duplicate columns.
      var mapper = new GridifyMapper<TestClass>(autoGenerateMappings: true);
      var lambda = Gridify.Builder.SelectExpressionBuilder<TestClass>.Build("Name,name", mapper);
      var compiled = lambda.Compile();
      var projected = compiled(new TestClass(1, "Alice", null));

      var props = projected.GetType().GetProperties();
      Assert.Single(props);
   }

   [Fact]
   public void Build_TwoLevelCollection_ErrorMentionsMultipleLevels()
   {
      var mapper = new GridifyMapper<TestClass>(autoGenerateMappings: true, maxNestingDepth: 3);
      var ex = Assert.Throws<GridifySelectException>(() =>
         Gridify.Builder.SelectExpressionBuilder<TestClass>.Build("children.children.name", mapper));
      Assert.Contains("collection", ex.Message, System.StringComparison.OrdinalIgnoreCase);
   }
}

public class GridifyQuerySelectTests
{
   [Fact]
   public void GridifyQuery_ImplementsIGridifySelecting()
   {
      IGridifySelecting q = new GridifyQuery { Select = "name,age" };
      Assert.Equal("name,age", q.Select);
   }

   [Fact]
   public void GridifyQuery_DefaultSelect_IsNull()
   {
      var q = new GridifyQuery();
      Assert.Null(q.Select);
   }
}

public class ApplySelectTests
{
   private static List<TestClass> SampleData() =>
   [
      new(1, "Alice", null),
      new(2, "Bob", null),
      new(3, "Carol", null)
   ];

   [Fact]
   public void ApplySelect_FlatProjection_ReturnsRequestedFields()
   {
      var data = SampleData().AsQueryable();
      var result = data.ApplySelect("name,id").ToList();

      Assert.Equal(3, result.Count);
      var first = result[0];
      Assert.NotNull(first.GetType().GetProperty("name"));
      Assert.NotNull(first.GetType().GetProperty("id"));
      Assert.Equal("Alice", first.GetType().GetProperty("name")!.GetValue(first));
   }

   [Fact]
   public void ApplySelect_NullOrWhitespace_ReturnsCastedQuery()
   {
      var data = SampleData().AsQueryable();
      Assert.Equal(3, data.ApplySelect((string?)null).Count());
      Assert.Equal(3, data.ApplySelect("").Count());
      Assert.Equal(3, data.ApplySelect("   ").Count());
   }

   [Fact]
   public void ApplySelect_IGridifySelectingOverload_Works()
   {
      var data = SampleData().AsQueryable();
      IGridifySelecting selecting = new GridifyQuery { Select = "name" };
      var result = data.ApplySelect(selecting).ToList();
      Assert.Equal(3, result.Count);
   }

   [Fact]
   public void ApplySelect_NullSelecting_ReturnsCastedQuery()
   {
      var data = SampleData().AsQueryable();
      Assert.Equal(3, data.ApplySelect((IGridifySelecting?)null).Count());
   }

   [Fact]
   public void ApplySelect_PartialMapperWithIgnoreNotMapped_DropsUnmappedFields()
   {
      var mapper = new GridifyMapper<TestClass>(c => c.IgnoreNotMappedFields = true)
         .AddMap("name", x => x.Name!);
      var data = SampleData().AsQueryable();

      var result = data.ApplySelect("name,id,doesNotExist", mapper).ToList();

      Assert.Equal(3, result.Count);
      var first = result[0];
      Assert.NotNull(first.GetType().GetProperty("name"));
      Assert.Null(first.GetType().GetProperty("id"));
      Assert.Null(first.GetType().GetProperty("doesNotExist"));
   }
}

public class GridifySelectPipelineTests
{
   private static List<TestClass> SampleData() =>
   [
      new(1, "Alice", null),
      new(2, "Bob", null),
      new(3, "Carol", null),
      new(4, "Dave", null),
      new(5, "Eve", null)
   ];

   [Fact]
   public void GridifySelect_AppliesFilterOrderingPagingAndProjection()
   {
      var data = SampleData().AsQueryable();
      var gq = new GridifyQuery
      {
         Filter = "id>1",
         OrderBy = "name",
         Page = 1,
         PageSize = 2,
         Select = "name,id"
      };
      Paging<object> result = data.GridifySelect(gq);

      Assert.Equal(4, result.Count); // filtered count (id > 1) = 4
      Assert.Equal(2, result.Data.Count()); // page size
      var first = result.Data.First();
      Assert.NotNull(first.GetType().GetProperty("name"));
   }

   [Fact]
   public void GridifySelect_NoSelect_ReturnsBoxedT()
   {
      var data = SampleData().AsQueryable();
      var gq = new GridifyQuery { Page = 1, PageSize = 10 };
      var result = data.GridifySelect(gq);

      Assert.Equal(5, result.Count);
      Assert.All(result.Data, item => Assert.IsType<TestClass>(item));
   }

   [Fact]
   public void GridifyQueryableSelect_ReturnsQueryablePagingOfObject()
   {
      var data = SampleData().AsQueryable();
      var gq = new GridifyQuery { Select = "name", Page = 1, PageSize = 10 };
      QueryablePaging<object> qp = data.GridifyQueryableSelect(gq);

      Assert.Equal(5, qp.Count);
      Assert.Equal(5, qp.Query.Count());
   }
}

public class IsValidSelectTests
{
   [Fact]
   public void IsValidSelect_ValidSelect_ReturnsTrue()
   {
      var s = new GridifyQuery { Select = "name,id" };
      Assert.True(((IGridifySelecting)s).IsValidSelect<TestClass>());
   }

   [Fact]
   public void IsValidSelect_NullOrEmpty_ReturnsTrue()
   {
      Assert.True(((IGridifySelecting)new GridifyQuery { Select = null }).IsValidSelect<TestClass>());
      Assert.True(((IGridifySelecting)new GridifyQuery { Select = "" }).IsValidSelect<TestClass>());
   }

   [Fact]
   public void IsValidSelect_BadSyntax_ReturnsFalseWithError()
   {
      IGridifySelecting s = new GridifyQuery { Select = "name,," };
      var ok = s.IsValidSelect<TestClass>(out var errors);
      Assert.False(ok);
      Assert.NotEmpty(errors);
   }

   [Fact]
   public void IsValidSelect_UnmappedField_ReturnsFalseWithError()
   {
      IGridifySelecting s = new GridifyQuery { Select = "doesNotExist" };
      var mapper = new GridifyMapper<TestClass>().AddMap("name", x => x.Name!);
      var ok = s.IsValidSelect(out var errors, mapper);
      Assert.False(ok);
      Assert.Contains(errors, e => e.Contains("doesNotExist"));
   }

   [Fact]
   public void GridifyQuery_IsValid_Composite_AlsoChecksSelect()
   {
      var gq = new GridifyQuery { Filter = "id=1", OrderBy = "name", Select = "doesNotExist" };
      var mapper = new GridifyMapper<TestClass>(autoGenerateMappings: true);
      Assert.False(gq.IsValid(mapper));
   }
}
