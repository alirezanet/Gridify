using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

/// <summary>
/// Example demonstrating the use case from GitHub Discussion #251
/// This shows how to reuse a GridifyMapper for nested objects across multiple entities
/// </summary>
public class Issue251ExampleTests
{
   // Example classes from the discussion
   public class Address
   {
      public string City { get; set; } = string.Empty;
      public string Country { get; set; } = string.Empty;
      public string Secret { get; set; } = string.Empty;
   }

   public class User
   {
      public string Email { get; set; } = string.Empty;
      public Address Address { get; set; } = new();
   }

   public class Company
   {
      public string Name { get; set; } = string.Empty;
      public Address Address { get; set; } = new();
   }

   /// <summary>
   /// This test demonstrates the exact scenario from the GitHub discussion:
   /// - Define an AddressGridifyMapper once
   /// - Reuse it in UserGridifyMapper and CompanyGridifyMapper
   /// - This ensures the Secret field is not exposed for filtering
   /// </summary>
   [Fact]
   public void ExampleFromDiscussion_ShouldWork()
   {
      // Step 1: Create a mapper for Address that only exposes City and Country
      var addressGridifyMapper = new GridifyMapper<Address>()
          .AddMap("city", q => q.City)
          .AddMap("country", q => q.Country);
      // Note: Secret is intentionally not mapped and won't be filterable

      // Step 2: Create a mapper for User that reuses the Address mapper
      var userGridifyMapper = new GridifyMapper<User>()
          .AddMap("email", q => q.Email)
          .AddNestedMapper(q => q.Address, addressGridifyMapper);

      // Step 3: Create a mapper for Company that also reuses the Address mapper
      var companyGridifyMapper = new GridifyMapper<Company>()
          .AddMap("companyName", q => q.Name)
          .AddNestedMapper(q => q.Address, addressGridifyMapper);

      // Test data
      var users = new List<User>
        {
            new() { Email = "user1@example.com", Address = new Address { City = "New York", Country = "USA", Secret = "secret1" } },
            new() { Email = "user2@example.com", Address = new Address { City = "London", Country = "UK", Secret = "secret2" } },
            new() { Email = "user3@example.com", Address = new Address { City = "New York", Country = "USA", Secret = "secret3" } }
        };

      var companies = new List<Company>
        {
            new() { Name = "TechCorp", Address = new Address { City = "San Francisco", Country = "USA", Secret = "secret4" } },
            new() { Name = "GlobalInc", Address = new Address { City = "London", Country = "UK", Secret = "secret5" } },
            new() { Name = "StartupX", Address = new Address { City = "Berlin", Country = "Germany", Secret = "secret6" } }
        };

      // Test filtering on User.Address.City
      var filteredUsers = users.AsQueryable()
          .ApplyFiltering("address.city=New York", userGridifyMapper)
          .ToList();

      Assert.Equal(2, filteredUsers.Count);
      Assert.All(filteredUsers, u => Assert.Equal("New York", u.Address.City));

      // Test filtering on Company.Address.Country
      var filteredCompanies = companies.AsQueryable()
          .ApplyFiltering("address.country=UK", companyGridifyMapper)
          .ToList();

      Assert.Single(filteredCompanies);
      Assert.Equal("London", filteredCompanies[0].Address.City);

      // Verify that Secret is NOT exposed for filtering in both mappers
      Assert.False(userGridifyMapper.HasMap("address.secret"),
          "Secret field should not be exposed in User mapper");
      Assert.False(companyGridifyMapper.HasMap("address.secret"),
          "Secret field should not be exposed in Company mapper");

      // Verify the expected fields ARE exposed
      Assert.True(userGridifyMapper.HasMap("address.city"));
      Assert.True(userGridifyMapper.HasMap("address.country"));
      Assert.True(companyGridifyMapper.HasMap("address.city"));
      Assert.True(companyGridifyMapper.HasMap("address.country"));
   }

   /// <summary>
   /// Demonstrates that the mapper can be defined once and reused across multiple entities,
   /// similar to how AutoMapper allows embedding DTOs
   /// </summary>
   [Fact]
   public void ReuseMapper_AcrossMultipleEntities_WithDifferentPrefixes()
   {
      // Define the address mapper once
      var addressMapper = new GridifyMapper<Address>()
          .AddMap("city", q => q.City)
          .AddMap("country", q => q.Country);

      // Reuse for User with default prefix "address"
      var userMapper = new GridifyMapper<User>()
          .AddMap("email", q => q.Email)
          .AddNestedMapper(q => q.Address, addressMapper);

      // Reuse for Company with custom prefix "location"
      var companyMapper = new GridifyMapper<Company>()
          .AddMap("name", q => q.Name)
          .AddNestedMapper("location", q => q.Address, addressMapper);

      // Verify User mapper uses "address." prefix
      Assert.True(userMapper.HasMap("address.city"));
      Assert.True(userMapper.HasMap("address.country"));

      // Verify Company mapper uses "location." prefix
      Assert.True(companyMapper.HasMap("location.city"));
      Assert.True(companyMapper.HasMap("location.country"));

      // Test filtering with different prefixes
      var users = new List<User>
        {
            new() { Email = "test@example.com", Address = new Address { City = "Paris", Country = "France" } }
        };

      var companies = new List<Company>
        {
            new() { Name = "FrenchCo", Address = new Address { City = "Paris", Country = "France" } }
        };

      var userResult = users.AsQueryable()
          .ApplyFiltering("address.city=Paris", userMapper)
          .ToList();
      Assert.Single(userResult);

      var companyResult = companies.AsQueryable()
          .ApplyFiltering("location.city=Paris", companyMapper)
          .ToList();
      Assert.Single(companyResult);
   }
}
