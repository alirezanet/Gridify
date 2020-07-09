using System.Collections.Generic;

namespace Gridify {
   public class Paging<T> {
      public int TotalItems { get; set; }
      public IEnumerable<T> Items { get; set; }
   }
}