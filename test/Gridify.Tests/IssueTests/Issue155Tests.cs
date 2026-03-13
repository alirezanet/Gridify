using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Gridify.Tests.IssueTests;

public class Issue155Tests
{
   [Fact]
   public void ComparingFiledAWithFieldB_ShouldReturnTheCorrectResult()
   {
      // arrange
      List<Item> items =
      [
         new Item("Item1", [new TimeSchedule(1, 2), new TimeSchedule(2, 3)]),
         new Item("Item2", [new TimeSchedule(1, 4), new TimeSchedule(4, 3)]),
         new Item("Item3", [new TimeSchedule(3, 2), new TimeSchedule(2, 3)]),
      ];
      var expected = items.AsQueryable().Where(x => x.Schedules.Any(s => s.End < s.Start));

      // act
      var actual = new QueryBuilder<Item>()
         .AddMap("start", p => p.Schedules.Select(x => x.Start))
         .AddMap("end", p => p.Schedules.Select(x => x.End))
         .AddCondition("end < (start)")
         .Build(items.AsQueryable());

      // assert
      Assert.Equal(expected.ToString(), actual.ToString());
   }

   private record Item(string Name, List<TimeSchedule> Schedules);
   private record TimeSchedule(int Start, int End);
}
