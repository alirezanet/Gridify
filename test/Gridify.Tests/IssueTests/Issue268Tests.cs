
using System.Linq;
using AutoBogus;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class TestClassGridifyMapper : GridifyMapper<TestClass>
{
   public TestClassGridifyMapper()
   {
      GenerateMappings()
         .AddMap("ChildName", e => e.ChildClass!.Name);
   }
}

public class Issue268Tests
{
   [Fact]
   public void Mapping_ShouldAllowNullProperty()
   {
      var fakeList = AutoFaker.Generate<TestClass>(10);
      fakeList.Add(new TestClass() { ChildClass = null });
      fakeList.Add(new TestClass() { ChildClass = new TestClass() });
      fakeList.Add(new TestClass() { ChildClass = new TestClass() { Name = "glacor" } });
      var mapper = new TestClassGridifyMapper();
      mapper.Configuration.AvoidNullReference = true;

      var result = fakeList.AsQueryable().ApplyFiltering("ChildName=glacor", mapper).Distinct().ToList();
      var result2 = fakeList.AsQueryable().ApplyFiltering("ChildName=", mapper).Distinct().ToList();
      Assert.Single(result);
      Assert.Equal(2, result2.Count);
   }
}
