using System;
using Microsoft.EntityFrameworkCore;

namespace EntityFrameworkIntegrationTests.cs
{
   public class MyDbContext : DbContext
   {
      public DbSet<User> Users { get; set; }

      protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
      {
         optionsBuilder.UseInMemoryDatabase("myTestDb");
         base.OnConfiguring(optionsBuilder);
      }
   }

   public class User
   {
      public int Id { get; set; }
      public string Name { get; set; }
      public DateTime CreateDate { get; set; }
      public Guid FkGuid { get; set; }
   }
}