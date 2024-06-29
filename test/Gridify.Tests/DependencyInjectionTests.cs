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

   [Fact]
   public void AddGridifyQueryBuilders_ShouldRegisterQueryBuilders()
   {
      // Arrange
      var services = new ServiceCollection();
      var assemblyMock = GetAssembly();

      // Act
      services.AddGridifyQueryBuilders(assemblyMock);

      // Assert
      var serviceProvider = services.BuildServiceProvider();
      serviceProvider.GetService<IQueryBuilder<TestEntity>>().Should().NotBeNull();
      serviceProvider.GetService<IQueryBuilder<TestEntity2>>().Should().NotBeNull();
      serviceProvider.GetService<TestQueryBuilder>().Should().BeNull();
      services.Count.Should().Be(2);
   }

   [Fact]
   public void AddGridify_ShouldRegisterBothMappersAndQueryBuilders()
   {
      // Arrange
      var services = new ServiceCollection();
      var assemblyMock = GetAssembly();

      // Act
      services.AddGridify(assemblyMock);

      // Assert
      var serviceProvider = services.BuildServiceProvider();
      serviceProvider.GetService<IGridifyMapper<TestEntity>>().Should().NotBeNull();
      serviceProvider.GetService<IGridifyMapper<TestEntity2>>().Should().NotBeNull();
      serviceProvider.GetService<TestMapper>().Should().BeNull();
      serviceProvider.GetService<IQueryBuilder<TestEntity>>().Should().NotBeNull();
      serviceProvider.GetService<IQueryBuilder<TestEntity2>>().Should().NotBeNull();
      serviceProvider.GetService<TestQueryBuilder>().Should().BeNull();
      services.Count.Should().Be(4);
   }
   private static Assembly GetAssembly()
   {
      var assemblyMock = Substitute.For<Assembly>();
      assemblyMock.GetTypes().Returns([
         typeof(TestMapper),
         typeof(AnotherTestMapper),
         typeof(NonGridifyMapper),
         typeof(TestQueryBuilder),
         typeof(AnotherTestQueryBuilder)
      ]);
      return assemblyMock;
   }
   public record TestEntity(string P1, string P2);

   public record TestEntity2(string P1, string P2);

   public class TestMapper : GridifyMapper<TestEntity>;

   public class AnotherTestMapper : GridifyMapper<TestEntity2>;

   public class TestQueryBuilder : QueryBuilder<TestEntity>;

   public class AnotherTestQueryBuilder : QueryBuilder<TestEntity2>;

   // Non-Gridify types for testing
   public class NonGridifyMapper;
}
