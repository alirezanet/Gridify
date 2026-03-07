using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests;

public class QueryBuilderNestedMapperTests
{
   [Fact]
   public void AddNestedMapper_WithIGridifyMapperInstance_NoPrefix_ShouldComposeMappings()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
         .AddMap("city", x => x.City)
         .AddMap("country", x => x.Country);

      var builder = new QueryBuilder<User>()
         .AddMap("email", x => x.Email)
         .AddNestedMapper(x => x.Address, addressMapper);

      var users = new List<User>
      {
         new() { Email = "john@example.com", Address = new Address { City = "London", Country = "UK" } },
         new() { Email = "jane@example.com", Address = new Address { City = "Paris", Country = "France" } },
         new() { Email = "bob@example.com", Address = new Address { City = "London", Country = "UK" } }
      };

      // Act
      builder.AddCondition("city=London");
      var result = builder.Build(users.AsQueryable()).ToList();

      // Assert
      Assert.Equal(2, result.Count);
      Assert.All(result, u => Assert.Equal("London", u.Address.City));
   }

   [Fact]
   public void AddNestedMapper_WithIGridifyMapperInstance_WithPrefix_ShouldComposeMappingsWithPrefix()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
         .AddMap("city", x => x.City)
         .AddMap("country", x => x.Country);

      var builder = new QueryBuilder<User>()
         .AddMap("email", x => x.Email)
         .AddNestedMapper("location", x => x.Address, addressMapper);

      var users = new List<User>
      {
         new() { Email = "john@example.com", Address = new Address { City = "Tokyo", Country = "Japan" } },
         new() { Email = "jane@example.com", Address = new Address { City = "Seoul", Country = "Korea" } }
      };

      // Act
      builder.AddCondition("location.city=Tokyo");
      var result = builder.Build(users.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("Tokyo", result[0].Address.City);
   }

   [Fact]
   public void AddNestedMapper_WithCustomMapperClass_NoPrefix_ShouldComposeMappings()
   {
      // Arrange
      var builder = new QueryBuilder<Company>()
         .AddMap("name", x => x.Name)
         .AddNestedMapper<AddressMapperClass>(x => x.Address);

      var companies = new List<Company>
      {
         new() { Name = "CompanyA", Address = new Address { City = "Berlin", Country = "Germany" } },
         new() { Name = "CompanyB", Address = new Address { City = "Paris", Country = "France" } },
         new() { Name = "CompanyC", Address = new Address { City = "Berlin", Country = "Germany" } }
      };

      // Act
      builder.AddCondition("country=Germany");
      var result = builder.Build(companies.AsQueryable()).ToList();

      // Assert
      Assert.Equal(2, result.Count);
      Assert.All(result, c => Assert.Equal("Germany", c.Address.Country));
   }

   [Fact]
   public void AddNestedMapper_WithCustomMapperClass_WithPrefix_ShouldComposeMappingsWithPrefix()
   {
      // Arrange
      var builder = new QueryBuilder<Company>()
         .AddMap("name", x => x.Name)
         .AddNestedMapper<AddressMapperClass>("location", x => x.Address);

      var companies = new List<Company>
      {
         new() { Name = "CompanyA", Address = new Address { City = "Berlin", Country = "Germany" } },
         new() { Name = "CompanyB", Address = new Address { City = "Paris", Country = "France" } }
      };

      // Act
      builder.AddCondition("location.city=Berlin");
      var result = builder.Build(companies.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("Berlin", result[0].Address.City);
   }

   [Fact]
   public void AddNestedMapper_ShouldLazyInitializeMapper_IfNotAlreadySet()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
         .AddMap("city", x => x.City);

      var builder = new QueryBuilder<User>();
      // Note: No mapper set initially

      var users = new List<User>
      {
         new() { Email = "john@example.com", Address = new Address { City = "London", Country = "UK" } },
         new() { Email = "jane@example.com", Address = new Address { City = "Paris", Country = "France" } }
      };

      // Act
      builder.AddNestedMapper(x => x.Address, addressMapper)
         .AddCondition("city=London");
      var result = builder.Build(users.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("London", result[0].Address.City);
   }

   [Fact]
   public void AddNestedMapper_ShouldWorkWithMultipleNestedMappers()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
         .AddMap("city", x => x.City);

      var contactMapper = new GridifyMapper<Contact>()
         .AddMap("phone", x => x.Phone);

      var builder = new QueryBuilder<User>()
         .AddMap("email", x => x.Email)
         .AddNestedMapper("addr", x => x.Address, addressMapper)
         .AddNestedMapper("contact", x => x.Contact, contactMapper);

      var users = new List<User>
      {
         new()
         {
            Email = "john@example.com",
            Address = new Address { City = "London" },
            Contact = new Contact { Phone = "1234567890" }
         },
         new()
         {
            Email = "jane@example.com",
            Address = new Address { City = "Paris" },
            Contact = new Contact { Phone = "0987654321" }
         }
      };

      // Act
      builder.AddCondition("addr.city=London,contact.phone=1234567890");
      var result = builder.Build(users.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("London", result[0].Address.City);
      Assert.Equal("1234567890", result[0].Contact.Phone);
   }

   [Fact]
   public void AddNestedMapper_WithOrderBy_ShouldApplyOrderingCorrectly()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
         .AddMap("city", x => x.City);

      var builder = new QueryBuilder<User>()
         .AddMap("email", x => x.Email)
         .AddNestedMapper(x => x.Address, addressMapper)
         .AddCondition("city=London|city=Paris")
         .AddOrderBy("city");

      var users = new List<User>
      {
         new() { Email = "alice@example.com", Address = new Address { City = "Paris" } },
         new() { Email = "bob@example.com", Address = new Address { City = "London" } },
         new() { Email = "charlie@example.com", Address = new Address { City = "Paris" } }
      };

      // Act
      var result = builder.Build(users.AsQueryable()).ToList();

      // Assert
      Assert.Equal(3, result.Count);
      Assert.Equal("London", result[0].Address.City);
      Assert.Equal("Paris", result[1].Address.City);
      Assert.Equal("Paris", result[2].Address.City);
   }

   [Fact]
   public void AddNestedMapper_WithPaging_ShouldApplyPagingCorrectly()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
         .AddMap("city", x => x.City);

      var builder = new QueryBuilder<User>()
         .AddMap("email", x => x.Email)
         .AddNestedMapper(x => x.Address, addressMapper)
         .AddCondition("city=London")
         .ConfigurePaging(0, 1); // Page 0, size 1

      var users = new List<User>
      {
         new() { Email = "john@example.com", Address = new Address { City = "London" } },
         new() { Email = "jane@example.com", Address = new Address { City = "London" } },
         new() { Email = "bob@example.com", Address = new Address { City = "London" } }
      };

      // Act
      var result = builder.Build(users.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("john@example.com", result[0].Email);
   }

   [Fact]
   public void AddNestedMapper_WithPagingAndCount_ShouldReturnCorrectCount()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
         .AddMap("city", x => x.City);

      var builder = new QueryBuilder<User>()
         .AddMap("email", x => x.Email)
         .AddNestedMapper(x => x.Address, addressMapper)
         .AddCondition("city=London")
         .ConfigurePaging(0, 2);

      var users = new List<User>
      {
         new() { Email = "john@example.com", Address = new Address { City = "London" } },
         new() { Email = "jane@example.com", Address = new Address { City = "London" } },
         new() { Email = "bob@example.com", Address = new Address { City = "London" } }
      };

      // Act
      var paging = builder.BuildWithPaging(users.AsQueryable());

      // Assert
      Assert.Equal(3, paging.Count);
      Assert.Equal(2, paging.Data.Count());
   }

   [Fact]
   public void AddNestedMapper_ShouldRespectOverrideFlag_WhenFalse()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
         .AddMap("city", x => x.City);

      var builder = new QueryBuilder<User>()
         .AddMap("email", x => x.Email)
         .AddMap("city", x => x.Email) // Pre-existing mapping
         .AddNestedMapper(x => x.Address, addressMapper, overrideIfExists: false);

      var users = new List<User>
      {
         new() { Email = "london@example.com", Address = new Address { City = "London" } },
         new() { Email = "paris@example.com", Address = new Address { City = "Paris" } }
      };

      // Act
      builder.AddCondition("city=london@example.com");
      var result = builder.Build(users.AsQueryable()).ToList();

      // Assert - should match email (pre-existing mapping), not city
      Assert.Single(result);
      Assert.Equal("london@example.com", result[0].Email);
   }

   [Fact]
   public void AddNestedMapper_ShouldRespectOverrideFlag_WhenTrue()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
         .AddMap("city", x => x.City);

      var builder = new QueryBuilder<User>()
         .AddMap("email", x => x.Email)
         .AddMap("city", x => x.Email) // Pre-existing mapping
         .AddNestedMapper(x => x.Address, addressMapper, overrideIfExists: true);

      var users = new List<User>
      {
         new() { Email = "london@example.com", Address = new Address { City = "London" } },
         new() { Email = "paris@example.com", Address = new Address { City = "Paris" } }
      };

      // Act
      builder.AddCondition("city=London");
      var result = builder.Build(users.AsQueryable()).ToList();

      // Assert - should match address.city (nested mapping overrides pre-existing)
      Assert.Single(result);
      Assert.Equal("London", result[0].Address.City);
   }

   [Fact]
   public void AddNestedMapper_WithCompiledEvaluator_ShouldWorkCorrectly()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
         .AddMap("city", x => x.City);

      var builder = new QueryBuilder<User>()
         .AddMap("email", x => x.Email)
         .AddNestedMapper(x => x.Address, addressMapper)
         .AddCondition("city=London");

      var users = new List<User>
      {
         new() { Email = "john@example.com", Address = new Address { City = "London" } },
         new() { Email = "jane@example.com", Address = new Address { City = "Paris" } }
      };

      // Act
      var result = builder.Build(users).ToList();

      // Assert
      Assert.Single(result);
      Assert.Equal("London", result[0].Address.City);
   }

   [Fact]
   public void AddNestedMapper_ChainMultipleCalls_ShouldApplyAllMappings()
   {
      // Arrange
      var addressMapper = new GridifyMapper<Address>()
         .AddMap("city", x => x.City);

      var contactMapper = new GridifyMapper<Contact>()
         .AddMap("phone", x => x.Phone);

      var builder = new QueryBuilder<User>()
         .AddMap("email", x => x.Email)
         .AddNestedMapper(x => x.Address, addressMapper)
         .AddNestedMapper(x => x.Contact, contactMapper);

      var users = new List<User>
      {
         new()
         {
            Email = "john@example.com",
            Address = new Address { City = "London" },
            Contact = new Contact { Phone = "123456" }
         }
      };

      // Act
      builder.AddCondition("city=London")
         .AddCondition("phone=123456");
      var result = builder.Build(users.AsQueryable()).ToList();

      // Assert
      Assert.Single(result);
   }

   // Helper mapper class
   public class AddressMapperClass : GridifyMapper<Address>
   {
      public AddressMapperClass()
      {
         AddMap("city", x => x.City, convertor: null);
         AddMap("country", x => x.Country, convertor: null);
      }
   }
}

// Test entities
public class User
{
   public string Email { get; set; } = string.Empty;
   public Address Address { get; set; } = new();
   public Contact Contact { get; set; } = new();
}

public class Address
{
   public string City { get; set; } = string.Empty;
   public string Country { get; set; } = string.Empty;
   public string Secret { get; set; } = string.Empty;
}

public class Company
{
   public string Name { get; set; } = string.Empty;
   public Address Address { get; set; } = new();
}

public class Contact
{
   public string Phone { get; set; } = string.Empty;
}

