using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class CaseInsensitiveNonStringFilterTest
{
   private static readonly Guid TestGuid = Guid.Parse("12345678-1234-1234-1234-123456789abc");

   private List<TestClass> DataSource => new()
    {
        new TestClass { Id = 1, MyGuid = TestGuid, IsActive = true },
        new TestClass { Id = 2, MyGuid = Guid.NewGuid(), IsActive = false },
        new TestClass { Id = 3, MyGuid = Guid.NewGuid(), IsActive = false },
    };

   [Fact]
   public void ApplyFiltering_WithCaseInsensitiveOperator_OnGuidProperty_ShouldNotThrow()
   {
      var actual = DataSource.AsQueryable()
          .ApplyFiltering($"MyGuid={TestGuid}/i")
          .ToList();

      Assert.Single(actual);
   }

   [Fact]
   public void ApplyFiltering_WithCaseInsensitiveOperator_OnIntProperty_ShouldNotThrow()
   {
      var actual = DataSource.AsQueryable()
          .ApplyFiltering("Id=1/i")
          .ToList();

      Assert.Single(actual);
   }

   [Fact]
   public void ApplyFiltering_WithCaseInsensitiveOperator_OnBoolProperty_ShouldNotThrow()
   {
      var actual = DataSource.AsQueryable()
          .ApplyFiltering("IsActive=true/i")
          .ToList();

      Assert.Single(actual);
   }

   [Fact]
   public void ApplyFiltering_WithGlobalCaseInsensitiveFiltering_OnNonStringProperties_ShouldNotThrow()
   {
      var mapper = new GridifyMapper<TestClass>(q => q.CaseInsensitiveFiltering = true)
          .GenerateMappings();

      Assert.Single(DataSource.AsQueryable().ApplyFiltering($"MyGuid={TestGuid}", mapper).ToList());
      Assert.Single(DataSource.AsQueryable().ApplyFiltering("Id=1", mapper).ToList());
      Assert.Single(DataSource.AsQueryable().ApplyFiltering("IsActive=true", mapper).ToList());
   }
}
