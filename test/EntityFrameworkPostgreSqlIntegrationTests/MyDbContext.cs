using System;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace EntityFrameworkIntegrationTests.cs;

public class MyDbContext : DbContext
{
   public DbSet<User> Users { get; set; }
   public DbSet<Products> ProductsViews { get; set; }

   protected override void OnModelCreating(ModelBuilder modelBuilder)
   {
      modelBuilder.Entity<User>().Property<string>("shadow1");
      base.OnModelCreating(modelBuilder);
   }

   protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
   {
      optionsBuilder.UseNpgsql("Server=myServerAddress;Database=myDataBase;User Id=myUsername;Password=myPassword;");
      optionsBuilder.AddInterceptors(new SuppressCommandResultInterceptor());
      optionsBuilder.AddInterceptors(new SuppressConnectionInterceptor());
      optionsBuilder.EnableServiceProviderCaching();

      base.OnConfiguring(optionsBuilder);
   }
}

public class User
{
   public int Id { get; set; }
   public string Name { get; set; } = string.Empty;
   public DateTime? CreateDate { get; set; }
   public Guid FkGuid { get; set; }
}
public class Products
{
   public int Id { get; set; }
   public string Name { get; set; } = string.Empty;

   [Column(TypeName = "jsonb")]
   public IEnumerable<ProductUser> Users { get; set; } = Enumerable.Empty<ProductUser>();
}

public class ProductUser
{
   public int Id { get; set; }
   public string Name { get; set; } = string.Empty;
}
