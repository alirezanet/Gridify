using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue81Tests
{
   [Fact]
   public void Filtering_WithConditionalOperatorAndWithTwoSelect_ShouldGenerateConditionalExpression()
   {
      // Arrange
      var qb = new QueryBuilder<RootClass>()
         .UseEmptyMapper()
         .AddMap("PostText", r => r.Blog1Id != null
            ? r.Blog1.Posts.Select(p => p.Text)
            : r.Blog2.Posts.Select(p => p.Text))
         .AddCondition("PostText = Hello");

      const string expected = "r => IIF((r.Blog1Id != null)," +
                              " ((r.Blog1.Posts != null) AndAlso r.Blog1.Posts.Any(p => (p.Text == \"Hello\")))," +
                              " ((r.Blog2.Posts != null) AndAlso r.Blog2.Posts.Any(p => (p.Text == \"Hello\"))))";

      // Act
      var actual = qb.BuildFilteringExpression().ToString();

      // Assert
      Assert.Equal(expected, actual);
   }

   [Fact]
   public void Filtering_WithConditionalOperatorAndWithSingleSelect_ShouldGenerateConditionalExpression()
   {
      // Arrange
      var qb = new QueryBuilder<RootClass>()
         .UseEmptyMapper()
         .AddMap("PostText", r => r.Blog1Id != null
            ? r.Blog1.Posts.Select(p => p.Text)
            : true)
         .AddCondition("PostText = Hello");

      const string expected = "r => IIF((r.Blog1Id != null)," +
                              " ((r.Blog1.Posts != null) AndAlso r.Blog1.Posts.Any(p => (p.Text == \"Hello\")))," +
                              " True)";

      // Act
      var actual = qb.BuildFilteringExpression().ToString();

      // Assert
      Assert.Equal(expected, actual);
   }

   [Fact]
   public void Filtering_WithConditionalOperatorAndWithSingleSelect2_ShouldGenerateConditionalExpression()
   {
      // Arrange
      var qb = new QueryBuilder<RootClass>()
         .UseEmptyMapper()
         .AddMap("PostText", r => r.Blog1Id != null
            ? true
            : r.Blog2.Posts.Select(p => p.Text))
         .AddCondition("PostText = Hello");

      const string expected = "r => IIF((r.Blog1Id != null)," +
                              " True," +
                              " ((r.Blog2.Posts != null) AndAlso r.Blog2.Posts.Any(p => (p.Text == \"Hello\"))))";

      // Act
      var actual = qb.BuildFilteringExpression().ToString();

      // Assert
      Assert.Equal(expected, actual);
   }

   [Fact]
   public void Filtering_WithConditionalOperator_ShouldGenerateConditionalExpression()
   {
      // Arrange
      var lst = new List<RootClass>()
         {
            new()
            {
               Blog1Id = 1, Blog2Id = 2,
               Blog2 = new Blog() { Posts = new List<Post>() { new() { Text = "Bye" } } },
               Blog1 = new Blog() { Posts = new List<Post>() { new() { Text = "Hello" } } }
            },
         }
         .AsQueryable();

      var qb = new QueryBuilder<RootClass>()
         .UseEmptyMapper()
         .AddMap("PostText", r => r.Blog1Id != null
            ? r.Blog1.Posts.Select(p => p.Text)
            : r.Blog2.Posts.Select(p => p.Text))
         .AddCondition("PostText = Hello");

      var expected = lst.Where(q => q.Blog1Id != null
         ? q.Blog1.Posts.Any(w => w.Text == "Hello")
         : q.Blog2.Posts.Any(w => w.Text == "Hello")).ToList();

      // Act
      var actual = qb.Build(lst).ToList();

      // Assert
      Assert.Equal(expected.Count, actual.Count);
      Assert.Equal(expected, actual);
      Assert.True(actual.Any());
   }
}

#nullable disable
public class RootClass
{
   public int Id { get; set; }

   public int? Blog1Id { get; set; }
   public Blog Blog1 { get; set; }

   public int? Blog2Id { get; set; }
   public Blog Blog2 { get; set; }
}

public class Blog
{
   public int Id { get; set; }
   public ICollection<Post> Posts { get; set; }
}

public class Post
{
   public int Id { get; set; }
   public string Text { get; set; }
}
