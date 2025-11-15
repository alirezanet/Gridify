using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue293Tests
{
   public class TestEntity
   {
      public int IntProperty { get; set; }
      public int? NullableIntProperty { get; set; }
      public string StringProperty { get; set; }
      public DateTime DateProperty { get; set; }
      public DateTime? NullableDateProperty { get; set; }
      public TestEnum EnumProperty { get; set; }
      public bool BoolProperty { get; set; }
      public Guid GuidProperty { get; set; }
      public decimal DecimalProperty { get; set; }
   }

   public enum TestEnum
   {
      Value1 = 1,
      Value2 = 2,
      Value3 = 3
   }

   [Fact]
   public void IsValid_WithValidIntValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "IntProperty=123" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithInvalidIntValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "IntProperty=xyz" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
      Assert.Contains("Cannot convert value 'xyz' to type 'Int32'", errors[0]);
   }

   [Fact]
   public void IsValid_WithValidDateTimeValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "DateProperty=2024-01-15" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithInvalidDateTimeValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "DateProperty=notadate" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
      Assert.Contains("Cannot convert value 'notadate' to type 'DateTime'", errors[0]);
   }

   [Fact]
   public void IsValid_WithValidEnumValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "EnumProperty=Value1" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithInvalidEnumValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "EnumProperty=InvalidValue" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
   }

   [Fact]
   public void IsValid_WithOverflowIntValue_ReturnsFalse()
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
   public void IsValid_WithValidBooleanValues_ReturnsTrue(string value)
   {
      var query = new GridifyQuery { Filter = $"BoolProperty={value}" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithInvalidBooleanValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "BoolProperty=notbool" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
   }

   [Fact]
   public void IsValid_WithValidGuidValue_ReturnsTrue()
   {
      var validGuid = Guid.NewGuid().ToString();
      var query = new GridifyQuery { Filter = $"GuidProperty={validGuid}" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithInvalidGuidValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "GuidProperty=not-a-guid" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
   }

   [Fact]
   public void IsValid_WithMultipleInvalidValues_ReturnsAllErrors()
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
   public void IsValid_WithNullValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "StringProperty=null" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithEmptyValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "StringProperty=" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithUnmappedField_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "NonExistentField=123" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.Contains("Field 'NonExistentField' is not mapped", errors[0]);
   }

   [Fact]
   public void IsValid_BackwardCompatibility_ExistingIsValidStillWorks()
   {
      var query = new GridifyQuery { Filter = "IntProperty=123" };
      var isValid = query.IsValid<TestEntity>();

      Assert.True(isValid);
   }

   [Fact]
   public void IsValid_WithDecimalValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "DecimalProperty=123.45" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithInvalidDecimalValue_ReturnsFalse()
   {
      var query = new GridifyQuery { Filter = "DecimalProperty=notanumber" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
   }

   [Fact]
   public void IsValid_WithComplexQuery_ValidatesCorrectly()
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
   public void IsValid_WithCustomMapper_ValidatesCorrectly()
   {
      var mapper = new GridifyMapper<TestEntity>()
          .AddMap("CustomInt", x => x.IntProperty);

      var query = new GridifyQuery { Filter = "CustomInt=xyz" };
      var isValid = query.IsValid(out var errors, mapper);

      Assert.False(isValid);
      Assert.NotEmpty(errors);
   }

   [Fact]
   public void IsValid_WithNullableIntAndValidValue_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "NullableIntProperty=123" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithNullableIntAndInvalidValue_ReturnsFalse()
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
   public void IsValid_WithDifferentOperatorsAndValidValues_ReturnsTrue(string filter)
   {
      var query = new GridifyQuery { Filter = filter };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithEmptyFilter_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = "" };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }

   [Fact]
   public void IsValid_WithNullFilter_ReturnsTrue()
   {
      var query = new GridifyQuery { Filter = null };
      var isValid = query.IsValid<TestEntity>(out var errors);

      Assert.True(isValid);
      Assert.Empty(errors);
   }
}
