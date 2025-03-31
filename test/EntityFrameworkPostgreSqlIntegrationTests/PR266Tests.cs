using EntityFrameworkIntegrationTests.cs;
using Gridify;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using Xunit;

namespace EntityFrameworkPostgreSqlIntegrationTests;

public class PR266Tests
{
   private readonly MyDbContext _dbContext = new();

   [Fact]
   public void ISO_Should_Convert_ToUTC()
   {
      var isoTimestamp = "2025-03-31T02:54:09Z";

      var mapper = new GridifyMapper<User>(q => q.DefaultDateTimeKind = DateTimeKind.Utc).GenerateMappings();
      var queryString = _dbContext.Users.ApplyFiltering($"CreateDate={isoTimestamp}", mapper).ToQueryString();

      var whereClause = queryString.Split("WHERE")[1].Trim();
      Assert.Equal($"u.\"CreateDate\" = TIMESTAMPTZ '{isoTimestamp}'", whereClause);
   }

   [Fact]
   public void ISO_Should_ShowcaseDifferentConversions_LocalToUtc()
   {
      var localDt = DateTime.Parse("2025-03-31T02:54:09Z", new CultureInfo("lt-LT"));
      Assert.Equal(DateTimeKind.Local, localDt.Kind);
      Assert.Equal("2025-03-31T05:54:09", localDt.ToString("s"));

      // Past behavior. Kind becomes Utc, but time is not converted.
      var utcDtWrong = DateTime.SpecifyKind(localDt, DateTimeKind.Utc);
      Assert.Equal(DateTimeKind.Utc, utcDtWrong.Kind);
      Assert.Equal("2025-03-31T05:54:09", utcDtWrong.ToString("s"));

      // Current behavior. Kind becomes Utc, and time is converted.
      var utcDtCorrect = localDt.ToUniversalTime();
      Assert.Equal(DateTimeKind.Utc, utcDtCorrect.Kind);
      Assert.Equal("2025-03-31T02:54:09", utcDtCorrect.ToString("s"));
   }

   [Fact]
   public void ISO_Should_ShowcaseDifferentConversions_UtcToLocal()
   {
      var utcDt = DateTime.Parse("2025-03-31T02:54:09Z", new CultureInfo("lt-LT")).ToUniversalTime();
      Assert.Equal(DateTimeKind.Utc, utcDt.Kind);
      Assert.Equal("2025-03-31T02:54:09", utcDt.ToString("s"));

      // Past behavior. Kind becomes Local, but time is not converted.
      var localDtWrong = DateTime.SpecifyKind(utcDt, DateTimeKind.Local);
      Assert.Equal(DateTimeKind.Local, localDtWrong.Kind);
      Assert.Equal("2025-03-31T02:54:09", localDtWrong.ToString("s"));

      // Current behavior. Kind becomes Local, and time is converted.
      var localDtCorrect = utcDt.ToLocalTime();
      Assert.Equal(DateTimeKind.Local, localDtCorrect.Kind);
      Assert.Equal("2025-03-31T05:54:09", localDtCorrect.ToString("s"));
   }
}
