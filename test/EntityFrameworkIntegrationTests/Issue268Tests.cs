using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using EntityFrameworkIntegrationTests.cs;
using Gridify.Syntax;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class TestClassGridifyMapper : GridifyMapper<User>
{
   public TestClassGridifyMapper()
   {
      GenerateMappings()
         .AddMap("FriendName", e => e.Friend!.Name);
   }
}

[Collection("Context collection")]
public class Issue268Tests : IClassFixture<DatabaseFixture>
{
   private readonly DatabaseFixture fixture;
   private MyDbContext _ctx => fixture._dbContext;
   public Issue268Tests(DatabaseFixture fixture)
   {
      this.fixture = fixture;
   }

   private void Mapping_ShouldAllowNullProperty(bool avoidNullReferences)
   {
      var userList = _ctx.Users;
      var mapper = new TestClassGridifyMapper();
      mapper.Configuration.AvoidNullReference = avoidNullReferences;

      var foreverAloneUsers = userList.Where(u => u.Friend == null).ToList();
      var uersWithNamelessFriends = userList.Where(u => u.Friend != null && string.IsNullOrEmpty(u.Friend.Name)).ToList();
      var weirdUsers = foreverAloneUsers.Concat(uersWithNamelessFriends)
         .ToList();

      var result = userList.AsQueryable().ApplyFiltering("FriendName=glacor", mapper).Distinct().ToList();
      var result2 = userList.AsQueryable().ApplyFiltering("FriendName=", mapper).Distinct().ToList();
      Assert.Single(result);
      Assert.Equal(weirdUsers.Count, result2.Count);
   }

   [Fact]
   public void Mapping_ShouldAllowNullProperty_WhenAvoidNullReferenceIsTrue()
   {
      Mapping_ShouldAllowNullProperty(true);
   }

   [Fact]
   public void Mapping_ShouldAllowNullProperty_WhenAvoidNullReferenceIsFalse()
   {
      Mapping_ShouldAllowNullProperty(false);
   }
}
