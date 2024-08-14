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
        Assert.True(mapper.HasMap("childClass.name"));
        
        var issueMapper = new GridifyMapper<Issue196TestClass>();
        issueMapper.GenerateMappings(2);
        Assert.True(issueMapper.HasMap("customFields.key"));
   }
}

public class Issue196TestClass
{    
    public required string Name { get; set; }

    public required CustomField CustomField { get; set; }

    public List<CustomField> CustomFields { get; set; } = [];
}

public class CustomField
{
    public required string Key { get; set; }
    public required string Value { get; set; }
}
