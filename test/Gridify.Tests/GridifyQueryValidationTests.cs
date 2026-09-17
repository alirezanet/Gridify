using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Gridify.Tests;

/// <summary>
/// Comprehensive tests for GridifyQuery validation functionality.
/// Tests both the basic IsValid() method and the detailed IsValid(out errors) overload.
/// </summary>
public class GridifyQueryValidationTests
{
   public class TestEntity
   {
      public int IntProperty { get; set; }
      public int? NullableIntProperty { get; set; }
      public string StringProperty { get; set; } = string.Empty;
      public DateTime DateProperty { get; set; }
      public DateTime? NullableDateProperty { get; set; }
      public TestEnum EnumProperty { get; set; }
      public bool BoolProperty { get; set; }
      public Guid GuidProperty { get; set; }
      public decimal DecimalProperty { get; set; }
      public int SecondIntProperty { get; set; }
      public long LongProperty { get; set; }
   }

   public enum TestEnum
   {
      Value1 = 1,
      Value2 = 2,
      Value3 = 3
   }

   #region Tests for IsValid(out List<string> errors) overload

   [Fact]
   public void IsValid_WithErrors_WithValidIntValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "IntProperty=123" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithInvalidIntValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "IntProperty=xyz" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
      Assert.Contains("Cannot convert value 'xyz' to type 'Int32'", errors[0]);
   }

   [Fact]
   public void IsValid_WithErrors_WithValidDateTimeValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "DateProperty=2024-01-15" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithInvalidDateTimeValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "DateProperty=notadate" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
      Assert.Contains("Cannot convert value 'notadate' to type 'DateTime'", errors[0]);
   }

   [Fact]
   public void IsValid_WithErrors_WithValidEnumValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "EnumProperty=Value1" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithInvalidEnumValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "EnumProperty=InvalidValue" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithOverflowIntValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "IntProperty=999999999999999" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
   }

   [Theory]
   [InlineData("true")]
   [InlineData("false")]
   [InlineData("1")]
   [InlineData("0")]
   public void IsValid_WithErrors_WithValidBooleanValues_ReturnsTrue(string value)
   {
      var query = new GridifyQuery { Filter = $"BoolProperty={value}" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithInvalidBooleanValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "BoolProperty=notbool" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithValidGuidValue_ReturnsTrue()
   {
      var validGuid = Guid.NewGuid().ToString();
      var query = new GridifyQuery { Filter = $"GuidProperty={validGuid}" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithInvalidGuidValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "GuidProperty=not-a-guid" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithMultipleInvalidValues_ReturnsAllErrors()
   {
      var query = new GridifyQuery
      {
         Filter = "IntProperty=xyz,DateProperty=notadate,EnumProperty=InvalidValue"
      };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.Equal(3, errors.Count);
   }

   [Fact]
   public void IsValid_WithErrors_WithNullValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "StringProperty=null" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithEmptyValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "StringProperty=" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithUnmappedField_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "NonExistentField=123" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.Contains("Field 'NonExistentField' is not mapped", errors[0]);
   }

   [Fact]
   public void IsValid_WithErrors_WithDecimalValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "DecimalProperty=123.45" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithInvalidDecimalValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "DecimalProperty=notanumber" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithComplexQuery_ValidatesCorrectly()
   {
      var query = new GridifyQuery
      {
         Filter = "(IntProperty>10,IntProperty<100)|StringProperty=test"
      };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithCustomMapper_ValidatesCorrectly()
   {
      var mapper = new GridifyMapper<TestEntity>()
          .AddMap("CustomInt", x => x.IntProperty);

      var query = new GridifyQuery { Filter = "CustomInt=xyz" };
      var isValid = query.IsValid(out var errors, mapper);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithNullableIntAndValidValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "NullableIntProperty=123" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithNullableIntAndInvalidValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "NullableIntProperty=xyz" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
   }

   [Theory]
   [InlineData("IntProperty>100")]
   [InlineData("IntProperty<100")]
   [InlineData("IntProperty>=100")]
   [InlineData("IntProperty<=100")]
   [InlineData("IntProperty!=100")]
   public void IsValid_WithErrors_WithDifferentOperatorsAndValidValues_ReturnsTrue(string filter)
   {
      var query = new GridifyQuery { Filter = filter };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithEmptyFilter_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithErrors_WithNullFilter_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = null };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   #endregion

   #region Tests for issue #337 - IsValid does not consider mapper's convertors

   /// <summary>
   /// A custom convertor that understands relative date expressions like "d-2" (2 days ago).
   /// </summary>
   private static object ConvertRelativeDateTime(string value)
   {
      if (value.StartsWith("d", StringComparison.OrdinalIgnoreCase) &&
          int.TryParse(value.Substring(1), out var days))
         return DateTime.Today.AddDays(days);

      return DateTime.Parse(value, CultureInfo.InvariantCulture);
   }

   // Reproduces issue #337:
   // the mapped property is a DateTime, and the value "d-2" is not a valid DateTime string,
   // but the map's convertor turns it into a DateTime. IsValid must consider the convertor.
   [Fact]
   public void IsValid_WithConvertor_ConvertibleValue_ReturnsTrue()
   {
      var mapper = new GridifyMapper<TestEntity>()
         .AddMap("createdOn", q => q.DateProperty, ConvertRelativeDateTime);

      var query = new GridifyQuery { Filter = "createdOn>d-2" };

      Assert.True(query.IsValid(mapper));
      Assert.True(query.IsValid(out var errors, mapper));
      Assert.Empty(errors);
   }

   // Proof that IsValid disagrees with the actual filtering behavior:
   // ApplyFiltering happily builds and runs the query, so IsValid returning false is a bug.
   [Fact]
   public void IsValid_WithConvertor_MustAgreeWithApplyFiltering()
   {
      var mapper = new GridifyMapper<TestEntity>()
         .AddMap("createdOn", q => q.DateProperty, ConvertRelativeDateTime);

      var source = new List<TestEntity>
      {
         new() { IntProperty = 1, DateProperty = DateTime.Today },
         new() { IntProperty = 2, DateProperty = DateTime.Today.AddDays(-10) }
      }.AsQueryable();

      var query = new GridifyQuery { Filter = "createdOn>d-2" };

      // the query works at runtime
      var actual = source.ApplyFiltering(query, mapper).ToList();
      Assert.Single(actual);
      Assert.Equal(1, actual[0].IntProperty);

      // ... so validation must not reject it
      Assert.True(query.IsValid(mapper));
   }

   // A convertor may also map arbitrary keywords onto a non-string property type.
   [Fact]
   public void IsValid_WithConvertor_NonStringTargetType_ReturnsTrue()
   {
      var mapper = new GridifyMapper<TestEntity>()
         .AddMap("size", q => q.IntProperty, value => value switch
         {
            "small" => 1,
            "medium" => 2,
            "large" => 3,
            _ => int.Parse(value)
         });

      var query = new GridifyQuery { Filter = "size=large" };

      Assert.True(query.IsValid(mapper));
   }

   // A convertor that throws for the value is still a validation error, the query builder
   // does not catch it either.
   [Fact]
   public void IsValid_WithConvertor_ThatThrows_ReturnsFalse()
   {
      var mapper = new GridifyMapper<TestEntity>()
         .AddMap("createdOn", q => q.DateProperty, ConvertRelativeDateTime);

      var query = new GridifyQuery { Filter = "createdOn>not-a-date" };

      Assert.False(query.IsValid(out var errors, mapper));
      Assert.NotEmpty(errors);
   }

   // The query builder uses a non-string convertor result as the value of the condition, so a
   // result it cannot use throws when the query is built. Validation has to say so rather than
   // report the filter as valid.
   [Fact]
   public void IsValid_WithConvertor_ReturningMismatchedType_ReturnsFalse()
   {
      // int for a long property
      var mapper = new GridifyMapper<TestEntity>()
         .AddMap("views", q => q.LongProperty, _ => 5);

      var query = new GridifyQuery { Filter = "views=five" };

      Assert.False(query.IsValid(out var errors, mapper));
      Assert.StartsWith("Cannot convert value 'five' for field 'views':", errors[0]);
      Assert.Throws<ArgumentException>(() => Source.ApplyFiltering(query, mapper).ToList());
   }

   [Fact]
   public void IsValid_WithConvertor_ReturningNullForNonNullableProperty_ReturnsFalse()
   {
      var mapper = new GridifyMapper<TestEntity>()
         .AddMap("anything", q => q.IntProperty, _ => null!);

      var query = new GridifyQuery { Filter = "anything=x" };

      Assert.False(query.IsValid(out var errors, mapper));
      Assert.NotEmpty(errors);
      Assert.Throws<ArgumentException>(() => Source.ApplyFiltering(query, mapper).ToList());
   }

   // The property that matters: whatever the convertor returns, IsValid has to agree with
   // whether ApplyFiltering can actually build the query. Every combination of convertor
   // result and property type, on both of the builder's value paths.
   [Theory]
   // --- plain LINQ, Expression.Constant needs the exact type ---
   [InlineData("long->long", false, true)]
   [InlineData("int->int", false, true)]
   [InlineData("int->nullableInt", false, true)]
   [InlineData("enum->enum", false, true)]
   [InlineData("dateTime->dateTime", false, true)]
   [InlineData("parsableString->int", false, true)]
   [InlineData("garbageString->int", false, false)]
   [InlineData("int->long", false, false)]
   [InlineData("long->int", false, false)]
   [InlineData("double->decimal", false, false)]
   [InlineData("int->decimal", false, false)]
   [InlineData("int->enum", false, false)]
   [InlineData("dateTime->int", false, false)]
   [InlineData("guid->int", false, false)]
   [InlineData("null->nullableInt", false, true)]
   [InlineData("null->string", false, true)]
   [InlineData("null->int", false, false)]
   [InlineData("null->dateTime", false, false)]
   // --- EF compatibility layer, the value is assigned through reflection, which widens ---
   [InlineData("long->long", true, true)]
   [InlineData("int->int", true, true)]
   [InlineData("int->long", true, true)]
   [InlineData("int->enum", true, true)]
   [InlineData("null->int", true, true)]
   [InlineData("null->nullableInt", true, true)]
   [InlineData("null->dateTime", true, true)]
   [InlineData("long->int", true, false)]
   [InlineData("double->decimal", true, false)]
   [InlineData("int->decimal", true, false)]
   [InlineData("dateTime->int", true, false)]
   [InlineData("guid->int", true, false)]
   public void IsValid_WithConvertor_AgreesWithWhetherTheQueryCanBeBuilt(
      string scenario, bool entityFrameworkCompatibilityLayer, bool expected)
   {
      var mapper = MapperFor(scenario, entityFrameworkCompatibilityLayer);
      var query = new GridifyQuery { Filter = "field=input" };

      Assert.Equal(expected, query.IsValid(mapper));

      // and the verdict has to match what ApplyFiltering actually does, whatever it throws
      var buildable = true;
      try { Source.ApplyFiltering(query, mapper).ToList(); }
      catch (Exception) { buildable = false; }

      Assert.Equal(buildable, query.IsValid(mapper));
   }

   // A composite map is ORed over every expression, so the convertor's result has to work for
   // all of them, not just the first one that IGMap.To exposes.
   [Fact]
   public void IsValid_WithConvertor_OnCompositeMapWithMixedPropertyTypes_ReturnsFalse()
   {
      var mapper = new GridifyMapper<TestEntity>();
      mapper.AddMap(new CompositeGMap<TestEntity>("mixed", (Func<string, object>)(_ => 1),
         q => q.IntProperty, q => q.StringProperty));

      var query = new GridifyQuery { Filter = "mixed=x" };

      Assert.False(query.IsValid(out var errors, mapper));
      Assert.NotEmpty(errors);
      Assert.Throws<ArgumentException>(() => Source.ApplyFiltering(query, mapper).ToList());
   }

   [Fact]
   public void IsValid_WithConvertor_OnCompositeMapWithMatchingPropertyTypes_ReturnsTrue()
   {
      var mapper = new GridifyMapper<TestEntity>();
      mapper.AddMap(new CompositeGMap<TestEntity>("either", (Func<string, object>)(_ => 1),
         q => q.IntProperty, q => q.SecondIntProperty));

      var query = new GridifyQuery { Filter = "either=x" };

      Assert.True(query.IsValid(mapper));
      Source.ApplyFiltering(query, mapper).ToList();
   }

   // A convertor is invoked exactly as often as filtering invokes it, so IsValid does not
   // amplify the cost or the side effects of an expensive convertor.
   [Fact]
   public void IsValid_WithConvertor_InvokesItOncePerValue()
   {
      var validationCalls = 0;
      var filteringCalls = 0;

      var query = new GridifyQuery { Filter = "field=a|field=b" };

      query.IsValid(new GridifyMapper<TestEntity>()
         .AddMap("field", q => q.IntProperty, _ => { validationCalls++; return 1; }));

      Source.ApplyFiltering(query, new GridifyMapper<TestEntity>()
         .AddMap("field", q => q.IntProperty, _ => { filteringCalls++; return 1; })).ToList();

      Assert.Equal(2, validationCalls);
      Assert.Equal(filteringCalls, validationCalls);
   }

   // Validation errors are the payload an API returns to its caller, so they have to name
   // the value the caller sent, not whatever the convertor turned it into.
   [Fact]
   public void IsValid_WithConvertor_ErrorMessage_QuotesTheOriginalInput()
   {
      var mapper = new GridifyMapper<TestEntity>()
         .AddMap("field", q => q.IntProperty, _ => "not-a-number");

      new GridifyQuery { Filter = "field=x" }.IsValid(out var errors, mapper);

      // the value the message is about is the one that came in, "x" and not "not-a-number".
      // The TypeConverter's own message is appended after it and does name the converted
      // value, which is useful context rather than a claim about the caller's input.
      Assert.StartsWith("Cannot convert value 'x' to type 'Int32' for field 'field':", errors[0]);
   }

   private static IGridifyMapper<TestEntity> MapperFor(string scenario, bool entityFrameworkCompatibilityLayer = false)
   {
      var mapper = new GridifyMapper<TestEntity>(q =>
         q.EntityFrameworkCompatibilityLayer = entityFrameworkCompatibilityLayer);

      return scenario switch
      {
         "long->long" => mapper.AddMap("field", q => q.LongProperty, _ => 5L),
         "int->int" => mapper.AddMap("field", q => q.IntProperty, _ => 5),
         "int->nullableInt" => mapper.AddMap("field", q => q.NullableIntProperty, _ => 5),
         "enum->enum" => mapper.AddMap("field", q => q.EnumProperty, _ => TestEnum.Value2),
         "dateTime->dateTime" => mapper.AddMap("field", q => q.DateProperty, _ => DateTime.Today),
         "parsableString->int" => mapper.AddMap("field", q => q.IntProperty, _ => "5"),
         "garbageString->int" => mapper.AddMap("field", q => q.IntProperty, _ => "nope"),
         "int->long" => mapper.AddMap("field", q => q.LongProperty, _ => 5),
         "long->int" => mapper.AddMap("field", q => q.IntProperty, _ => 5L),
         "double->decimal" => mapper.AddMap("field", q => q.DecimalProperty, _ => 1.5d),
         "int->decimal" => mapper.AddMap("field", q => q.DecimalProperty, _ => 5),
         "int->enum" => mapper.AddMap("field", q => q.EnumProperty, _ => 2),
         "dateTime->int" => mapper.AddMap("field", q => q.IntProperty, _ => DateTime.Today),
         "guid->int" => mapper.AddMap("field", q => q.IntProperty, _ => Guid.Empty),
         "null->nullableInt" => mapper.AddMap("field", q => q.NullableIntProperty, _ => null!),
         "null->string" => mapper.AddMap("field", q => q.StringProperty, _ => null!),
         "null->int" => mapper.AddMap("field", q => q.IntProperty, _ => null!),
         "null->dateTime" => mapper.AddMap("field", q => q.DateProperty, _ => null!),
         _ => throw new ArgumentOutOfRangeException(nameof(scenario), scenario, null)
      };
   }

   private static readonly IQueryable<TestEntity> Source =
      new List<TestEntity> { new() { IntProperty = 5, LongProperty = 5 } }.AsQueryable();

   #endregion

   #region Tests for issue #332 - IsValid(out errors) on IGridifyOrdering and IGridifyQuery

   private static IGridifyMapper<TestEntity> OrderingMapper() =>
      new GridifyMapper<TestEntity>()
         .AddMap("Name", x => x.StringProperty)
         .AddMap("Age", x => x.IntProperty);

   // Reproduces issue #332: on the same object, the error collecting overload used to bind to
   // the IGridifyFiltering extension and validate only Filter, so it returned true where the
   // boolean overload returned false.
   [Fact]
   public void IsValid_WithErrors_OnGridifyQuery_ValidatesOrderByToo()
   {
      var mapper = OrderingMapper();
      var query = new GridifyQuery(1, 100, "Name=Ivan", "ageee");

      Assert.False(query.IsValid(mapper));
      Assert.False(query.IsValid(out var errors, mapper));
      Assert.Contains("Field 'ageee' is not mapped", errors);
   }

   // The two overloads must never disagree about the verdict, whatever combination of a
   // good or bad Filter and OrderBy they are given.
   [Theory]
   [InlineData("Name=Ivan", "Age", true)]
   [InlineData("Name=Ivan", "ageee", false)]
   [InlineData("nope=Ivan", "Age", false)]
   [InlineData("nope=Ivan", "ageee", false)]
   [InlineData("Age=abc", "Age", false)]
   [InlineData(null, "Age", true)]
   [InlineData("Name=Ivan", null, true)]
   [InlineData(null, null, true)]
   [InlineData("Name=Ivan", "Age desc", true)]
   [InlineData("Name=Ivan", "Age bogus", false)]
   public void IsValid_WithErrors_OnGridifyQuery_AgreesWithTheBooleanOverload(
      string? filter, string? orderBy, bool expected)
   {
      var mapper = OrderingMapper();
      var query = new GridifyQuery { Filter = filter, OrderBy = orderBy };

      Assert.Equal(expected, query.IsValid(mapper));
      Assert.Equal(expected, query.IsValid(out var errors, mapper));
      Assert.Equal(expected, errors.Count == 0);
   }

   // Errors from both parts are reported, not just the ones from whichever fails first.
   [Fact]
   public void IsValid_WithErrors_OnGridifyQuery_CollectsFilterAndOrderingErrors()
   {
      var query = new GridifyQuery { Filter = "NonExistentField=1", OrderBy = "AnotherMissing" };

      Assert.False(query.IsValid<TestEntity>(out var errors));
      Assert.Equal(2, errors.Count);
      Assert.Contains("Field 'NonExistentField' is not mapped", errors);
      Assert.Contains("Field 'AnotherMissing' is not mapped", errors);
   }

   // One unmapped field is one problem, however many places it is used.
   [Fact]
   public void IsValid_WithErrors_WhenAFieldIsUnmappedInBothFilterAndOrderBy_ReportsItOnce()
   {
      var query = new GridifyQuery { Filter = "NonExistentField=1", OrderBy = "NonExistentField" };

      Assert.False(query.IsValid<TestEntity>(out var errors));
      Assert.Equal("Field 'NonExistentField' is not mapped", Assert.Single(errors));
   }

   [Theory]
   [InlineData("NonExistentField, NonExistentField")]
   [InlineData("NonExistentField, NonExistentField desc")]
   [InlineData("NonExistentField desc, NonExistentField")]
   public void IsValid_WithErrors_OnOrdering_WithTheSameUnmappedFieldTwice_ReportsItOnce(string orderBy)
   {
      var ordering = new GridifyQuery { OrderBy = orderBy };

      Assert.False(((IGridifyOrdering)ordering).IsValid<TestEntity>(out var errors));
      Assert.Equal("Field 'NonExistentField' is not mapped", Assert.Single(errors));
   }

   // ... but distinct fields are still reported separately.
   [Fact]
   public void IsValid_WithErrors_WithDifferentUnmappedFields_ReportsEachOne()
   {
      var query = new GridifyQuery { Filter = "MissingA=1", OrderBy = "MissingB" };

      Assert.False(query.IsValid<TestEntity>(out var errors));
      Assert.Equal(2, errors.Count);
      Assert.Contains("Field 'MissingA' is not mapped", errors);
      Assert.Contains("Field 'MissingB' is not mapped", errors);
   }

   [Fact]
   public void IsValid_WithErrors_OnGridifyQuery_WithBothValid_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "IntProperty=5", OrderBy = "IntProperty" };

      Assert.True(query.IsValid<TestEntity>(out var errors));
      Assert.Empty(errors);
   }

   [Theory]
   [InlineData("IntProperty", true)]
   [InlineData("IntProperty desc", true)]
   [InlineData("IntProperty, StringProperty", true)]
   [InlineData("IntProperty?, StringProperty!", true)]
   [InlineData("", true)]
   [InlineData("   ", true)]
   [InlineData(null, true)]
   [InlineData("NonExistentField", false)]
   [InlineData("IntProperty, NonExistentField", false)]
   public void IsValid_WithErrors_OnOrdering_MatchesTheBooleanOverload(string? orderBy, bool expected)
   {
      var ordering = new GridifyQuery { OrderBy = orderBy };

      Assert.Equal(expected, ((IGridifyOrdering)ordering).IsValid<TestEntity>());
      Assert.Equal(expected, ((IGridifyOrdering)ordering).IsValid<TestEntity>(out var errors));
      Assert.Equal(expected, errors.Count == 0);
   }

   [Fact]
   public void IsValid_WithErrors_OnOrdering_WithUnmappedField_NamesTheField()
   {
      var ordering = new GridifyQuery { OrderBy = "NonExistentField" };

      Assert.False(((IGridifyOrdering)ordering).IsValid<TestEntity>(out var errors));
      Assert.Equal("Field 'NonExistentField' is not mapped", Assert.Single(errors));
   }

   // ParseOrderings is a lazy iterator that throws GridifyOrderingException part way through
   // the enumeration, so the message has to be captured rather than the exception escaping.
   [Fact]
   public void IsValid_WithErrors_OnOrdering_WithMalformedOrdering_ReportsTheReason()
   {
      var ordering = new GridifyQuery { OrderBy = "IntProperty bogus" };

      Assert.False(((IGridifyOrdering)ordering).IsValid<TestEntity>(out var errors));
      Assert.StartsWith("Ordering validation error:", Assert.Single(errors));
      Assert.Contains("expected 'desc' or 'asc'", errors[0]);
   }

   // A malformed ordering after a valid one still reports the failure, and keeps the errors
   // already collected from the fields before it.
   [Fact]
   public void IsValid_WithErrors_OnOrdering_WithErrorsBeforeAMalformedOrdering_KeepsBoth()
   {
      var ordering = new GridifyQuery { OrderBy = "NonExistentField, IntProperty bogus" };

      Assert.False(((IGridifyOrdering)ordering).IsValid<TestEntity>(out var errors));
      Assert.Equal(2, errors.Count);
      Assert.Contains("Field 'NonExistentField' is not mapped", errors);
      Assert.StartsWith("Ordering validation error:", errors[1]);
   }

   [Fact]
   public void IsValid_WithErrors_OnOrdering_WithoutAMapper_UsesTheAutoGeneratedOne()
   {
      var mapped = new GridifyQuery { OrderBy = "IntProperty" };
      var unmapped = new GridifyQuery { OrderBy = "NotAProperty" };

      Assert.True(((IGridifyOrdering)mapped).IsValid<TestEntity>(out var noErrors));
      Assert.Empty(noErrors);

      Assert.False(((IGridifyOrdering)unmapped).IsValid<TestEntity>(out var errors));
      Assert.NotEmpty(errors);
   }

   #endregion

   #region Tests for backward compatible IsValid() method

   [Fact]
   public void IsValid_WithValidIntValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "IntProperty=123" };
      var isValid = query.IsValid<TestEntity>();

      Assert.True(isValid);
   }

   [Fact]
   public void IsValid_WithInvalidIntValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "IntProperty=xyz" };
      var isValid = query.IsValid<TestEntity>();

      Assert.False(isValid);
   }

   [Fact]
   public void IsValid_WithValidDateTimeValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "DateProperty=2024-01-15" };
      var isValid = query.IsValid<TestEntity>();

      Assert.True(isValid);
   }

   [Fact]
   public void IsValid_WithInvalidDateTimeValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "DateProperty=notadate" };
      var isValid = query.IsValid<TestEntity>();

      Assert.False(isValid);
   }

   [Fact]
   public void IsValid_WithValidEnumValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "EnumProperty=Value1" };
      var isValid = query.IsValid<TestEntity>();

      Assert.True(isValid);
   }

   [Fact]
   public void IsValid_WithInvalidEnumValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "EnumProperty=InvalidValue" };
      var isValid = query.IsValid<TestEntity>();

      Assert.False(isValid);
   }

   [Fact]
   public void IsValid_WithUnmappedField_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "NonExistentField=123" };
      var isValid = query.IsValid<TestEntity>();

      Assert.False(isValid);
   }

   [Fact]
   public void IsValid_WithEmptyFilter_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "" };
      var isValid = query.IsValid<TestEntity>();

      Assert.True(isValid);
   }

   [Fact]
   public void IsValid_WithNullFilter_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = null };
      var isValid = query.IsValid<TestEntity>();

      Assert.True(isValid);
   }

   [Fact]
   public void IsValid_WithCustomMapper_ValidatesCorrectly()
   {
      var mapper = new GridifyMapper<TestEntity>()
          .AddMap("CustomInt", x => x.IntProperty);

      var query = new GridifyQuery { Filter = "CustomInt=xyz" };
      var isValid = query.IsValid(mapper);

      Assert.False(isValid);
   }

   #endregion
}
























