using System;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EntityFrameworkIntegrationTests.cs;

public class MyDbContext : DbContext
{
   public DbSet<User> Users { get; set; }

   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
      optionsBuilder.UseSqlServer("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;");
      optionsBuilder.AddInterceptors(new SuppressCommandResultInterceptor());
      optionsBuilder.AddInterceptors(new SuppressConnectionInterceptor());
      optionsBuilder.EnableServiceProviderCaching();
      base.OnConfiguring(optionsBuilder);
   }
}

public class User
{
   public int Id { get; set; }
   public string Name { get; set; }
   public DateTime? CreateDate { get; set; }
   public Guid FkGuid { get; set; }
}