using System;
using System.Linq;
using Xunit;

namespace EntityFrameworkIntegrationTests.cs
{
   public class DatabaseFixture : IDisposable
   {
      public DatabaseFixture()
      {
         _dbContext = new MyDbContext();
         AddTestUsers();
         // ... initialize data in the test database ...
      }

      public void Dispose()
      {
         _dbContext.Dispose();
      }

      public MyDbContext _dbContext { get; private set; }
      private void AddTestUsers()
      {
         Assert.Equal(0,_dbContext.Users.Count());
         _dbContext.Users.AddRange(
            new User { Id = 1, Name = "ahmad" },
            new User { Id = 2, Name = "ali" },
            new User { Id = 3, Name = "vahid" },
            new User { Id = 4, Name = "hamid" },
            new User { Id = 5, Name = "Hamed" },
            new User { Id = 6, Name = "sara" },
            new User { Id = 7, Name = "Ali" });

         _dbContext.SaveChanges();
      }
   }
}
