using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue202Tests
{
    private readonly List<TestClass> _fakeRepository = [.. GridifyExtensionsShould.GetSampleData()];
   
    [Fact]
    // Issue #202
    public void ApplyFiltering_NotExists_GlobalCaseInsensitiveSearch()
    {
        var mapper = new GridifyMapper<TestClass>(m => m.CaseInsensitiveFiltering = true).GenerateMappings();

        var gq = new GridifyQuery { Filter = "tag=" };

        var expected = _fakeRepository.Where(q => string.IsNullOrEmpty(q.Tag)).ToList();

        var actual = _fakeRepository.AsQueryable()
            .ApplyFiltering(gq, mapper)
            .ToList();

        Assert.Equal(expected.Count, actual.Count);
        Assert.Equal(expected, actual);
        Assert.True(actual.Any());
    }
   
    [Fact]
    // Issue #202
    public void ApplyFiltering_Exists_GlobalCaseInsensitiveSearch()
    {
        var mapper = new GridifyMapper<TestClass>(m => m.CaseInsensitiveFiltering = true).GenerateMappings();

        var gq = new GridifyQuery { Filter = "tag!=" };

        var expected = _fakeRepository.Where(q => !string.IsNullOrEmpty(q.Tag)).ToList();

        var actual = _fakeRepository.AsQueryable()
            .ApplyFiltering(gq, mapper)
            .ToList();

        Assert.Equal(expected.Count, actual.Count);
        Assert.Equal(expected, actual);
        Assert.True(actual.Any());
    }
}