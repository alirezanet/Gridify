using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests;

public class Issue124Tests
{
   [Fact]
   public void GenerateMappings_WhenMaxNestingDepthIsNotZero_ShouldGenerateMappingsForSubClasses()
   {
      var gm = new GridifyMapper<TestClass>()
         .GenerateMappings(2);

      Assert.NotNull(gm.GetGMap("Id")); // NestingLevel = 0
      Assert.NotNull(gm.GetGMap("ChildClass.Id")); // NestingLevel = 1
      Assert.NotNull(gm.GetGMap("ChildClass.ChildClass.Id")); // NestingLevel = 2
      Assert.Null(gm.GetGMap("ChildClass.ChildClass.ChildClass.Id")); // NestingLevel = 3
   }

   [Fact]
   public void GenerateMappings_WhenMaxNestingDepthIsZero_ShouldNotGenerateMappingsForSubClasses()
   {
      var gm = new GridifyMapper<TestClass>()
         .GenerateMappings();

      Assert.NotNull(gm.GetGMap("Id")); // NestingLevel = 0
      Assert.Null(gm.GetGMap("ChildClass.Id")); // NestingLevel = 1
      Assert.Null(gm.GetGMap("ChildClass.ChildClass.Id")); // NestingLevel = 2
      Assert.Null(gm.GetGMap("ChildClass.ChildClass.ChildClass.Id")); // NestingLevel = 3
   }

   [Fact]
   public void GenerateMappings_WhenChildClassTypeIsDifferent_ShouldMapTheNestedClasses()
   {
      var mapper = new GridifyMapper<Person>()
         .GenerateMappings(1);

      Assert.True(mapper.HasMap("Contact.Address"));
      var mappedClass = mapper.GetCurrentMaps().Any(q => q.From.Equals("contact", StringComparison.InvariantCultureIgnoreCase));
      Assert.False(mappedClass);
   }

   [Fact]
   public void GenerateMappings_WhenMapperUsedInFiltering_ShouldReturnCorrectResult()
   {
      var query = new List<Person>()
      {
         new() { FirstName = "A", Contact = new Contact() { Address = "AddressA" } },
         new() { FirstName = "B", Contact = new Contact() { Address = "AddressB" } },
         new() { FirstName = "C", Contact = new Contact() { Address = "AddressC" } }
      }.AsQueryable();

      var mapper = new GridifyMapper<Person>().GenerateMappings(1);

      var result = query.ApplyFiltering("contact.address=AddressB", mapper).ToList();
      Assert.True(result.Count == 1);
      Assert.True(result.First().FirstName == "B");
   }


   public class Person
   {
      public string UserName { get; set; }
      public string FirstName { get; set; }
      public string LastName { get; set; }
      public string Password { get; set; }
      public Contact Contact { get; set; }

   }

   public class Contact
   {
      public string Address { get; set; }
      public int PhoneNumber { get; set; }
   }

}
