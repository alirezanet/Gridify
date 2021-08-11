using System.Collections.Generic;

namespace Gridify
{
   public class Paging<T>
   {
      public Paging()
      {
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