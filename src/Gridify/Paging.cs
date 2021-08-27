using System.Collections.Generic;
using System.Linq;

namespace Gridify
{
   public class Paging<T>
   {
      public Paging()
      {
         Data = Enumerable.Empty<T>();
      }

      public Paging(int count,IEnumerable<T> data)
      {
         Count = count;
         Data = data;
      }
      public int Count { get; set; }
      public IEnumerable<T> Data { get; set; }
   }
}