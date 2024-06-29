using System.Reflection;
using FluentAssertions;
using Microsoft.Extensions.DependencyInjection;
using NSubstitute;
using Xunit;

namespace Gridify.Tests;

public class DependencyInjectionTests
{

   [Fact]
   public void AddGridifyMappers_ShouldRegisterMappers()
   {
      // Arrange
      var services = new ServiceCollection();
      var assemblyMock = GetAssembly();

      // Act
      services.AddGridifyMappers(assemblyMock);

      // Assert
      var serviceProvider = services.BuildServiceProvider();
      serviceProvider.GetService<IGridifyMapper<TestEntity>>().Should().NotBeNull();
      serviceProvider.GetService<IGridifyMapper<TestEntity2>>().Should().NotBeNull();
      serviceProvider.GetService<TestMapper>().Should().BeNull();
      services.Count.Should().Be(2);
   }

   private static Assembly GetAssembly()
   {
      var assemblyMock = Substitute.For<Assembly>();
      assemblyMock.GetTypes().Returns([
         typeof(TestMapper),
         typeof(AnotherTestMapper),
         typeof(NonGridifyMapper),
      ]);
      return assemblyMock;
   }
   public record TestEntity(string P1, string P2);

   public record TestEntity2(string P1, string P2);

   public class TestMapper : GridifyMapper<TestEntity>;

   public class AnotherTestMapper : GridifyMapper<TestEntity2>;


   // Non-Gridify types for testing
   public class NonGridifyMapper;
}
