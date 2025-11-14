using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue286Tests
{

   [Fact]
   public void NullableOrderingWithCustomMappings()
   {
      // arrange
      var gm = new GridifyMapper<Test>();
      gm.AddMap("sent", x => x.ClientSentDate);
      var query = new List<Test>().AsQueryable();

      // act
      query.ApplyOrdering("sent?", gm);
   }

   [Fact]
   public void NullableOrderingWithoutCustomMappings()
   {
      // arrange
      var query = new List<Test>().AsQueryable();

      // act
      query.ApplyOrdering("ClientSentDate?");
   }

   class Test
   {
      public DateTime? ClientSentDate { get; set; }
   }
}
