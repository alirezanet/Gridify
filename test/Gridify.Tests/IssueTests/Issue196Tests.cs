using System.Collections.Generic;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue196Tests
{
    [Fact]
    public void MapperShouldNotThrowExceptionWhenMappingToCollection()
    {
        var mapper = new GridifyMapper<TestClass>();
        mapper.GenerateMappings(2);
        // test that no exception is thrown
        Assert.True(true);
        
        var issueMapper = new GridifyMapper<Issue196TestClass>();
        issueMapper.GenerateMappings(2);
        // test that no exception is thrown
        Assert.True(true);
    }
}

public class Issue196TestClass
{    
    public string Name { get; set; }
    // DOES NOT WORK
    // public CustomFields CustomFields { get; set; }
	
    // WORDS
    public List<CustomField> CustomFields { get; set; }
}

public class CustomField
{
    public string Key { get; init; }
    public string Value { get; set; }
	
    public CustomField(string key, string value)
    {
        Key = key;
        Value = value;
    }
    public CustomField() { }
}