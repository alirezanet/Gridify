using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue171Tests
{

   private readonly List<TestClass> _fakeRepository = [.. GridifyExtensionsShould.GetSampleData(), new TestClass(4444, "/i", null)];

   [Fact]
   public void ApplyFiltering_CaseInsensitiveEmptyFieldsWithParenthesis_ShouldFilter()
   {
      var actual = _fakeRepository.AsQueryable().ApplyFiltering("(tag=/i)").ToList();
      var expected = _fakeRepository.Where(q => string.IsNullOrEmpty(q.Tag)).ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void ApplyFiltering_CaseInsensitiveEmptyFields_ShouldFilter()
   {
      var actual = _fakeRepository.AsQueryable().ApplyFiltering("tag=/i").ToList();
      var expected = _fakeRepository.Where(q => string.IsNullOrEmpty(q.Tag)).ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void ApplyFiltering_CaseInsensitiveEmptyFieldsWithSpaces_ShouldFilter()
   {
      var actual = _fakeRepository.AsQueryable().ApplyFiltering("tag= /i").ToList();
      var expected = _fakeRepository.Where(q => string.IsNullOrEmpty(q.Tag)).ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

   [Fact]
   public void ApplyFiltering_EscapedCaseInsensitive_ShouldFilter()
   {
      var actual = _fakeRepository.AsQueryable().ApplyFiltering(@"name=\/i").ToList();
      var expected = _fakeRepository.Where(q => q.Name == "/i").ToList();

      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }

}
