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
}
