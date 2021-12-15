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
            var expected = context.Customers.ApplyFilteringAndOrdering(gm).ToList();
            var actual = context.Customers
               .Where(q=>q.Id > 3)
               .OrderByDescending(q => q.Name)
               .ToList();
            Assert.True(actual.Any());
            Assert.Equal(expected.First(),actual.First());
            Assert.Equal(expected,actual);
         }
      }

      [Fact] // Issue #58
      public void NestedCollectionFiltering_EntityFramework6_Or_NetFramework_ShouldNotThrowAnyExceptions()
      {
         var connection = GetConnection();

         var builder = new QueryBuilder<Customer>()
            .AddMap("name2", q => q.SubCollection.Select(c => c.Name))
            .AddCondition("name2=*X");

         // normal list (.NetFramework)
         var list = FakeCustomers().ToList();

         var actual2 = list.Where(q => q.SubCollection != null &&
                                       q.SubCollection.Any(w=>w.Name.Contains("X"))).ToList();

         // this should have null checks like actual2
         // since EntityFrameworkCompatibilityLayer is disabled by default.
         var expected2 = builder.Build(list).ToList();

         Assert.True(actual2.Any());
         Assert.Equal(expected2.First(),actual2.First());
         Assert.Equal(expected2,actual2);

         // DbContext (EF6)
         GridifyGlobalConfiguration.EnableEntityFrameworkCompatibilityLayer();
         using (var context = new EntityContext(connection))
         {
            var expected = builder.Build(context.Customers).ToList();

            var actual = context.Customers
               .Where(q => q.SubCollection.Any(w=>w.Name.Contains("X")))
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
            var list = FakeCustomers();

            context.Customers.AddRange(list);
            context.SaveChanges();
         }

         return connection;
      }

      private static IEnumerable<Customer> FakeCustomers()
      {
         var list = new List<Customer>();
         for (var i = 0; i < 10; i++)
         {
            list.Add(new Customer { Name = "ZZZ_" + i });
            list.Add(new Customer { Name = "ZZZ_" + i });
         }

         list.Add(new Customer()
         {
            Name = "sub",
            SubCollection = new List<Customer>() { new Customer() { Name = "sub_sub" } }
         });
         list.Add(new Customer()
         {
            Name = "sub",
            SubCollection = new List<Customer>() { new Customer() { Name = "XX" } }
         });
         return list;
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
         public ICollection<Customer> SubCollection { get; set; }
      }
   }
}
