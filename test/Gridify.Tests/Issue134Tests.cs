using System;
using System.Globalization;
using Xunit;

namespace Gridify.Tests;

public class Issue134Tests
{
   [Fact]
   public void GridifyMapperConvertor_WhenDateTimeIsParsed_ShouldKeepDateTimeKind()
   {
      // arrange
      var dateTimeStringUtc = "2023-11-14T14:36:15.615Z";
      var qb = new QueryBuilder<TestClass>()
          .AddMap("dt", q => q.MyDateTime, value => DateTime.Parse(value, null, DateTimeStyles.AdjustToUniversal))
         .AddCondition($"dt={dateTimeStringUtc}");

      var expected = DateTime.Parse(dateTimeStringUtc, null, DateTimeStyles.AdjustToUniversal)
                        .ToString(CultureInfo.CurrentCulture);

      // act
      var actual = qb.BuildFilteringExpression().ToString();

      // assert
      Assert.Contains(expected, actual);
   }
}
