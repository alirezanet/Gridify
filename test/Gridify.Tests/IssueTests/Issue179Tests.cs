using System;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Gridify.Tests.IssueTests;

/// <summary>
/// Tests for Issue #179 - Thread Safety in GridifyGlobalConfiguration
/// </summary>
public class Issue179Tests
{
   [Fact]
   public async Task ConcurrentConfigurationChanges_ShouldNotInterfere()
   {
      // Arrange
      var tasks = new Task[10];
      var results = new bool[10];
      var lockObj = new object();

      // Act - Multiple threads changing configuration concurrently
      for (int i = 0; i < 10; i++)
      {
         var index = i;
         var expectedValue = i % 2 == 0; // Alternate between true/false

         tasks[i] = Task.Run(async () =>
         {
            // Each thread sets its own value
            GridifyGlobalConfiguration.DisableNullChecks = expectedValue;

            // Simulate some work
            await Task.Delay(10);

            // Verify the value is still what we set
            var actualValue = GridifyGlobalConfiguration.DisableNullChecks;

            lock (lockObj)
            {
               results[index] = actualValue == expectedValue;
            }
         });
      }

      await Task.WhenAll(tasks);

      // Assert - All threads should have maintained their own values
      Assert.All(results, result => Assert.True(result,
          "Configuration value was overwritten by another thread"));
   }

   [Fact]
   public async Task ParallelTests_WithDifferentConfigurations_ShouldNotInterfere()
   {
      // This simulates the scenario described in Issue #179
      // where one test changes config and affects other tests

      var test1Task = Task.Run(async () =>
      {
         GridifyGlobalConfiguration.DisableNullChecks = true;
         GridifyGlobalConfiguration.AllowNullSearch = true;
         await Task.Delay(50);

         Assert.True(GridifyGlobalConfiguration.DisableNullChecks);
         Assert.True(GridifyGlobalConfiguration.AllowNullSearch);
      });

      var test2Task = Task.Run(async () =>
      {
         GridifyGlobalConfiguration.DisableNullChecks = false;
         GridifyGlobalConfiguration.AllowNullSearch = false;
         await Task.Delay(50);

         Assert.False(GridifyGlobalConfiguration.DisableNullChecks);
         Assert.False(GridifyGlobalConfiguration.AllowNullSearch);
      });

      var test3Task = Task.Run(async () =>
      {
         GridifyGlobalConfiguration.DefaultPageSize = 100;
         GridifyGlobalConfiguration.CaseInsensitiveFiltering = true;
         await Task.Delay(50);

         Assert.Equal(100, GridifyGlobalConfiguration.DefaultPageSize);
         Assert.True(GridifyGlobalConfiguration.CaseInsensitiveFiltering);
      });

      // All tests should pass without interference
      await Task.WhenAll(test1Task, test2Task, test3Task);
   }

   [Fact]
   public void ResetToDefaults_ShouldResetAllValues()
   {
      // Arrange - Set various configurations
      GridifyGlobalConfiguration.DisableNullChecks = true;
      GridifyGlobalConfiguration.AllowNullSearch = false;
      GridifyGlobalConfiguration.DefaultPageSize = 100;
      GridifyGlobalConfiguration.CaseInsensitiveFiltering = true;
      GridifyGlobalConfiguration.EntityFrameworkCompatibilityLayer = true;

      // Act
      GridifyGlobalConfiguration.ResetToDefaults();

      // Assert - All should return to defaults
      Assert.False(GridifyGlobalConfiguration.DisableNullChecks);
      Assert.True(GridifyGlobalConfiguration.AllowNullSearch);
      Assert.Equal(20, GridifyGlobalConfiguration.DefaultPageSize);
      Assert.False(GridifyGlobalConfiguration.CaseInsensitiveFiltering);
      Assert.False(GridifyGlobalConfiguration.EntityFrameworkCompatibilityLayer);
   }

   [Fact]
   public async Task CustomOperators_InDifferentThreads_ShouldBeIsolated()
   {
      // Arrange
      var operator1 = new Syntax.OperatorManager();
      var operator2 = new Syntax.OperatorManager();

      var task1Result = false;
      var task2Result = false;

      // Act
      var task1 = Task.Run(() =>
      {
         GridifyGlobalConfiguration.SetCustomOperators(operator1);
         var retrieved = GridifyGlobalConfiguration.CustomOperators;
         task1Result = ReferenceEquals(retrieved, operator1);
      });

      var task2 = Task.Run(() =>
      {
         GridifyGlobalConfiguration.SetCustomOperators(operator2);
         var retrieved = GridifyGlobalConfiguration.CustomOperators;
         task2Result = ReferenceEquals(retrieved, operator2);
      });

      await Task.WhenAll(task1, task2);

      // Assert
      Assert.True(task1Result, "Task 1 should get its own operator manager");
      Assert.True(task2Result, "Task 2 should get its own operator manager");
   }

   [Fact]
   public async Task AsyncContextFlow_ShouldMaintainConfiguration()
   {
      // Arrange
      GridifyGlobalConfiguration.DisableNullChecks = true;
      GridifyGlobalConfiguration.DefaultPageSize = 50;

      // Act - Configuration should flow through async context
      await Task.Run(async () =>
      {
         // Should maintain the configuration set in parent context
         Assert.True(GridifyGlobalConfiguration.DisableNullChecks);
         Assert.Equal(50, GridifyGlobalConfiguration.DefaultPageSize);

         await Task.Delay(10);

         // Still should maintain after await
         Assert.True(GridifyGlobalConfiguration.DisableNullChecks);
         Assert.Equal(50, GridifyGlobalConfiguration.DefaultPageSize);
      });

      // Cleanup
      GridifyGlobalConfiguration.ResetToDefaults();
   }

   [Fact]
   public void NestedAsyncContexts_ShouldIsolateProperly()
   {
      // Parent context
      GridifyGlobalConfiguration.DisableNullChecks = false;
      GridifyGlobalConfiguration.DefaultPageSize = 20;

      Assert.False(GridifyGlobalConfiguration.DisableNullChecks);
      Assert.Equal(20, GridifyGlobalConfiguration.DefaultPageSize);

      // Child context with different values
      Task.Run(() =>
      {
         GridifyGlobalConfiguration.DisableNullChecks = true;
         GridifyGlobalConfiguration.DefaultPageSize = 100;

         Assert.True(GridifyGlobalConfiguration.DisableNullChecks);
         Assert.Equal(100, GridifyGlobalConfiguration.DefaultPageSize);
      }).Wait();

      // Parent context should not be affected
      Assert.False(GridifyGlobalConfiguration.DisableNullChecks);
      Assert.Equal(20, GridifyGlobalConfiguration.DefaultPageSize);

      // Cleanup
      GridifyGlobalConfiguration.ResetToDefaults();
   }
}
