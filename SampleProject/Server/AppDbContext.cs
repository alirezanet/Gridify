using Microsoft.EntityFrameworkCore;
using SampleProject.Entites;

namespace SampleProject
{
   public class AppDbContext : DbContext
   {
      public AppDbContext(DbContextOptions<AppDbContext> options)
         : base(options)
      { }

      public DbSet<Person> People { get; set; }
      public DbSet<Contact> Contacts { get; set; }
   }
}
