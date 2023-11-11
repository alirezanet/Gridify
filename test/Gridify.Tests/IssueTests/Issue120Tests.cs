using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue120Tests
{
   [Fact]
   public void ApplyFiltering_WhenPassedADoubleValue_ShouldWorkCorrectly()
   {
      CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo("en");
      var query = new List<Test>()
      {
         new() { PropertyDouble = 0.2 },
         new() { PropertyDouble = 0.6 },
         new() { PropertyDouble = 0 },
      }.AsQueryable();

      var result = query.ApplyFiltering("propertyDouble > 0.5").ToList();

      Assert.Single(result);
   }

   private class Test
   {
      public double PropertyDouble { get; set; }
   }
}
