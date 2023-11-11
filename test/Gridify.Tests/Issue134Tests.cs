using System;
using System.Globalization;
using System.Reflection;
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

      var expected = DateTime.Parse(dateTimeStringUtc, null, DateTimeStyles.AdjustToUniversal);
      var expectedKind = expected.Kind;

      // act
      var actual = qb.BuildFilteringExpression();

      // assert
      var compiled = actual.Compile();
      var constants = (object[])compiled.Target!.GetType().GetField("Constants")!.GetValue(compiled.Target)!;
      var actualKind = ((DateTime)constants[0]).Kind;
      Assert.Equal(expectedKind,actualKind);
      Assert.Contains(expected.ToString(CultureInfo.CurrentCulture), actual.ToString());
   }
}
