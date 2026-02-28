using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

// Related to Issue #251 - Reuse GridifyMapper
public class Issue251Tests
{
   // Test data classes matching the example from the discussion
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

   [Fact]
   public void AddNestedMapper_ShouldReuseAddressMapper_ForUserEntity()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
          .AddMap("city", x => x.City)
          .AddMap("country", x => x.Country);
      // Note: Secret is not mapped, so it should not be filterable

      var userMapper = new GridifyMapper<User>()
          .AddMap("email", x => x.Email)
          .AddNestedMapper(x => x.Address, addressMapper);

      var users = new List<User>
        {
            new() { Email = "john@example.com", Address = new Address { City = "London", Country = "UK", Secret = "secret1" } },
            new() { Email = "jane@example.com", Address = new Address { City = "Paris", Country = "France", Secret = "secret2" } },
            new() { Email = "bob@example.com", Address = new Address { City = "London", Country = "UK", Secret = "secret3" } }
        };

      // Act - filter by address city
      var result = users.AsQueryable()
          .ApplyFiltering("address.city=London", userMapper)
          .ToList();

      // Assert
      Assert.Equal(2, result.Count);
      Assert.All(result, u => Assert.Equal("London", u.Address.City));
   }

   [Fact]
   public void AddNestedMapper_ShouldReuseAddressMapper_ForCompanyEntity()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
          .AddMap("city", x => x.City)
          .AddMap("country", x => x.Country);

      var companyMapper = new GridifyMapper<Company>()
          .AddMap("companyName", x => x.Name)
          .AddNestedMapper(x => x.Address, addressMapper);

      var companies = new List<Company>
        {
            new() { Name = "CompanyA", Address = new Address { City = "Berlin", Country = "Germany", Secret = "secret1" } },
            new() { Name = "CompanyB", Address = new Address { City = "Paris", Country = "France", Secret = "secret2" } },
            new() { Name = "CompanyC", Address = new Address { City = "Berlin", Country = "Germany", Secret = "secret3" } }
        };

      // Act - filter by address country
      var result = companies.AsQueryable()
          .ApplyFiltering("address.country=Germany", companyMapper)
          .ToList();

      // Assert
      Assert.Equal(2, result.Count);
      Assert.All(result, c => Assert.Equal("Germany", c.Address.Country));
   }

   [Fact]
   public void AddNestedMapper_WithCustomPrefix_ShouldUseProvidedPrefix()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
          .AddMap("city", x => x.City)
          .AddMap("country", x => x.Country);

      var userMapper = new GridifyMapper<User>()
          .AddMap("email", x => x.Email)
          .AddNestedMapper(x => x.Address, addressMapper, prefix: "location");

      var users = new List<User>
        {
            new() { Email = "john@example.com", Address = new Address { City = "Tokyo", Country = "Japan" } },
            new() { Email = "jane@example.com", Address = new Address { City = "Seoul", Country = "Korea" } }
        };

      // Act - filter using custom prefix
      var result = users.AsQueryable()
          .ApplyFiltering("location.city=Tokyo", userMapper)
          .ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("Tokyo", result[0].Address.City);
   }

   [Fact]
   public void AddNestedMapper_ShouldNotExposeUnmappedProperties()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
          .AddMap("city", x => x.City)
          .AddMap("country", x => x.Country);
      // Secret is intentionally not mapped

      var userMapper = new GridifyMapper<User>()
          .AddMap("email", x => x.Email)
          .AddNestedMapper(x => x.Address, addressMapper);

      // Act & Assert - verify that "address.secret" is not mapped
      Assert.False(userMapper.HasMap("address.secret"));
      Assert.True(userMapper.HasMap("address.city"));
      Assert.True(userMapper.HasMap("address.country"));
   }

   [Fact]
   public void AddNestedMapper_WithMultipleConditions_ShouldWorkCorrectly()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
          .AddMap("city", x => x.City)
          .AddMap("country", x => x.Country);

      var userMapper = new GridifyMapper<User>()
          .AddMap("email", x => x.Email)
          .AddNestedMapper(x => x.Address, addressMapper);

      var users = new List<User>
        {
            new() { Email = "john@example.com", Address = new Address { City = "London", Country = "UK" } },
            new() { Email = "jane@example.com", Address = new Address { City = "Paris", Country = "France" } },
            new() { Email = "bob@example.com", Address = new Address { City = "London", Country = "UK" } },
            new() { Email = "alice@example.com", Address = new Address { City = "London", Country = "France" } }
        };

      // Act - filter by both city and country
      var result = users.AsQueryable()
          .ApplyFiltering("address.city=London,address.country=UK", userMapper)
          .ToList();

      // Assert
      Assert.Equal(2, result.Count);
      Assert.All(result, u =>
      {
         Assert.Equal("London", u.Address.City);
         Assert.Equal("UK", u.Address.Country);
      });
   }

   [Fact]
   public void AddNestedMapper_WithConvertor_ShouldApplyConversion()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
          .AddMap("city", x => x.City, v => v.ToUpper())  // Convert filter value to uppercase
          .AddMap("country", x => x.Country);

      var userMapper = new GridifyMapper<User>()
          .AddMap("email", x => x.Email)
          .AddNestedMapper(x => x.Address, addressMapper);

      var users = new List<User>
        {
            new() { Email = "john@example.com", Address = new Address { City = "LONDON", Country = "UK" } },
            new() { Email = "jane@example.com", Address = new Address { City = "PARIS", Country = "France" } }
        };

      // Act - filter with lowercase, should match uppercase due to convertor
      var result = users.AsQueryable()
          .ApplyFiltering("address.city=paris", userMapper)
          .ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("PARIS", result[0].Address.City);
   }

   [Fact]
   public void AddNestedMapper_WithCompositeMap_ShouldComposeCorrectly()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
          .AddCompositeMap("search", x => x.City, x => x.Country);

      var userMapper = new GridifyMapper<User>()
          .AddMap("email", x => x.Email)
          .AddNestedMapper(x => x.Address, addressMapper);

      var users = new List<User>
        {
            new() { Email = "john@example.com", Address = new Address { City = "London", Country = "UK" } },
            new() { Email = "jane@example.com", Address = new Address { City = "Paris", Country = "France" } },
            new() { Email = "bob@example.com", Address = new Address { City = "Berlin", Country = "Germany" } }
        };

      // Act - search should work on both city and country
      var resultCity = users.AsQueryable()
          .ApplyFiltering("address.search=Paris", userMapper)
          .ToList();

      var resultCountry = users.AsQueryable()
          .ApplyFiltering("address.search=Germany", userMapper)
          .ToList();

      // Assert
      Assert.Single(resultCity);
      Assert.Equal("Paris", resultCity[0].Address.City);

      Assert.Single(resultCountry);
      Assert.Equal("Berlin", resultCountry[0].Address.City);
   }

   [Fact]
   public void AddNestedMapper_ShouldNotOverrideExistingMaps_WhenOverrideIfExistsIsFalse()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
          .AddMap("city", x => x.City);

      var userMapper = new GridifyMapper<User>()
          .AddMap("address.city", x => x.Email) // Intentionally map to email first
          .AddNestedMapper(x => x.Address, addressMapper, overrideIfExists: false);

      // Assert - the original mapping should remain
      var map = userMapper.GetGMap("address.city");
      Assert.NotNull(map);

      // The expression should still point to Email, not Address.City
      var expression = map.To.ToString();
      Assert.Contains("Email", expression);
   }
}
