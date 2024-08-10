using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue191Tests
{
   private readonly List<TestClass> _fakeRepository = [.. GridifyExtensionsShould.GetSampleData()];

   [Fact]
   public void ApplyFiltering_GlobalCaseInsensitiveSearch()
   {
      var mapper = new GridifyMapper<TestClass>(m => m.CaseInsensitiveFiltering = true).GenerateMappings();

      var gq = new GridifyQuery { Filter = "name=BOB" };

      var expected = _fakeRepository.Where(q => q.Name!.ToLower() == "BOB".ToLower()).ToList();

      var actual = _fakeRepository.AsQueryable()
          .ApplyFiltering(gq, mapper)
          .ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }
}
