using EntityFrameworkIntegrationTests.cs;
using Gridify;
using Microsoft.EntityFrameworkCore;
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
      var localDt = DateTime.Parse("2025-03-31T02:54:09Z"); // parsed as Local, value adjusted from Z
      Assert.Equal(DateTimeKind.Local, localDt.Kind);

      // Past behavior: flips Kind only
      var utcDtWrong = DateTime.SpecifyKind(localDt, DateTimeKind.Utc);
      Assert.Equal(DateTimeKind.Utc, utcDtWrong.Kind);
      Assert.Equal(localDt, utcDtWrong);

      // Current behavior: converts to UTC using offset at that moment
      var utcDtCorrect = localDt.ToUniversalTime();
      Assert.Equal(DateTimeKind.Utc, utcDtCorrect.Kind);

      var offsetAtThatTime = TimeZoneInfo.Local.GetUtcOffset(localDt);
      Assert.Equal(localDt - offsetAtThatTime, utcDtCorrect);
   }

   [Fact]
   public void ISO_Should_ShowcaseDifferentConversions_UtcToLocal()
   {
      var utcDt = DateTime.Parse("2025-03-31T02:54:09Z").ToUniversalTime();
      Assert.Equal(DateTimeKind.Utc, utcDt.Kind);

      // Past behavior: flips Kind only
      var localDtWrong = DateTime.SpecifyKind(utcDt, DateTimeKind.Local);
      Assert.Equal(DateTimeKind.Local, localDtWrong.Kind);
      Assert.Equal(utcDt, localDtWrong);

      // Current behavior: converts to Local using offset at that moment
      var localDtCorrect = utcDt.ToLocalTime();
      Assert.Equal(DateTimeKind.Local, localDtCorrect.Kind);

      var offsetAtThatTime = TimeZoneInfo.Local.GetUtcOffset(utcDt); // offset for that UTC instant
      Assert.Equal(utcDt + offsetAtThatTime, localDtCorrect);
   }

}
