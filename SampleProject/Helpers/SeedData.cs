using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using SampleProject.Models;
using System;

namespace SampleProject.Helpers
{
   public class SeedData
   {
      public static void Initialize(IServiceProvider serviceProvider)
      {
         using (var context = new AppDbContext(
             serviceProvider.GetRequiredService<DbContextOptions<AppDbContext>>()))
         {
            context.People.AddRange(
               new Person { Id = 1, FirstName = "Alireza", LastName = "Sabouri", Age = 30 },
               new Person { Id = 2, FirstName = "Aria", LastName = "Dark", Age = 25 },
               new Person { Id = 3, FirstName = "Sajjad", LastName = "Spawn", Age = 25 },
               new Person { Id = 4, FirstName = "Danial", LastName = "DeVi", Age = 23 },
               new Person { Id = 5, FirstName = "Ali", LastName = "Karp", Age = 24 },
               new Person { Id = 6, FirstName = "Alireza", LastName = "Protective", Age = 40 });
            context.Contacts.AddRange(
               new Contact { Id = 1, Address = "2673  Kincheloe Road", PhoneNumber = 123456789, PersonId = 1 },
               new Contact { Id = 2, Address = "741  Fancher Drive", PhoneNumber = 234567890, PersonId = 2 },
               new Contact { Id = 3, Address = "2079  Jarvis Street", PhoneNumber = 345678901, PersonId = 3 },
               new Contact { Id = 4, Address = "4644  Jennifer Lane", PhoneNumber = 456789012, PersonId = 4 },
               new Contact { Id = 5, Address = "4677  John Lane", PhoneNumber = 567890123, PersonId = 5 },
               new Contact { Id = 6, Address = "4664  John Calvin Drive", PhoneNumber = 123644342, PersonId = 6 });
            context.SaveChanges();
         }
      }
   }
}