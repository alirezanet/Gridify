using System.Collections.Generic;
using System.Data.Common;
using System.Data.Entity;
using System.Linq;
using Effort;
using Effort.Provider;
using Gridify;
using Xunit;

namespace EntityFramework6IntegrationTests.cs
{
   public class EntityFramework6Tests
   {
      [Fact]
      public void EntityFramework6_OrderBySupport() // Issue #8
      {
         var connection = GetConnection();

         using (var context = new EntityContext(connection))
         {
            var gm = new GridifyQuery() { OrderBy = "name desc" };
            var expected = context.Customers.ApplyOrdering(gm).ToList();
            var actual = context.Customers
                           .OrderByDescending(q => q.Name)
                           .ToList();
            Assert.True(actual.Any());
            Assert.Equal(expected.First(),actual.First());
            Assert.Equal(expected,actual);
         }
      }
      
      [Fact]
      public void EntityFramework6_OrderBySupportAscending() // Issue #8
      {
         var connection = GetConnection();

         using (var context = new EntityContext(connection))
         {
            var gm = new GridifyQuery() { OrderBy = "name" };
            var expected = context.Customers.ApplyOrdering(gm).ToList();
            var actual = context.Customers
               .OrderBy(q => q.Name)
               .ToList();
            Assert.True(actual.Any());
            Assert.Equal(expected.First(),actual.First());
            Assert.Equal(expected,actual);
         }
      }

      [Fact]
      public void EntityFramework6_FilteringAndOrdering() // Issue #8
      {
         var connection = GetConnection();

         using (var context = new EntityContext(connection))
         {
            var gm = new GridifyQuery() { OrderBy = "name desc" , Filter = "id > 3"};
            var expected = context.Customers.ApplyFilterAndOrdering(gm).ToList();
            var actual = context.Customers
               .Where(q=>q.Id > 3)   
               .OrderByDescending(q => q.Name)
               .ToList();
            Assert.True(actual.Any());
            Assert.Equal(expected.First(),actual.First());
            Assert.Equal(expected,actual);
         }
      }

      private static EffortConnection GetConnection()
      {
         var connection = DbConnectionFactory.CreateTransient();
         using (var context = new EntityContext(connection))
         {
            var list = new List<Customer>();
            for (var i = 0; i < 10; i++)
            {
               list.Add(new Customer {Name = "ZZZ_" + i});
            }

            context.Customers.AddRange(list);
            context.SaveChanges();
         }

         return connection;
      }

      private class EntityContext : DbContext
      {
         public EntityContext(DbConnection connection) : base(connection, false)
         {
         }

         public DbSet<Customer> Customers { get; set; }
      }

      private class Customer
      {
         public int Id { get; set; }
         public string Name { get; set; }
      }
   }
}