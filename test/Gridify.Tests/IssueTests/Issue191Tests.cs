﻿using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue191Tests
{
    private readonly List<TestClass> _fakeRepository = new(GridifyExtensionsShould.GetSampleData());

    [Fact]
    public void ApplyFiltering_GlobalCaseInsensitiveSearch() //issue #21
    {
        GridifyGlobalConfiguration.DefaultStringComparisonIsCaseInsensitive = true;
        
        var gq = new GridifyQuery { Filter = "name=BOB" };

        var actual = _fakeRepository.AsQueryable()
            .ApplyFiltering(gq)
            .ToList();

        var expected = _fakeRepository.Where(q => q.Name!.ToLower() == "BOB".ToLower()).ToList();
        Assert.Equal(expected.Count, actual.Count);
        Assert.Equal(expected, actual);
        Assert.True(actual.Any());
        
        GridifyGlobalConfiguration.DefaultStringComparisonIsCaseInsensitive = false;
    }
}
