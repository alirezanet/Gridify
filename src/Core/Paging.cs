using System.Collections.Generic;

namespace Gridify
{
   public class Paging<T>
   {
      public Paging()
      {
      }

      public Paging(int totalItems,IEnumerable<T> items)
      {
         TotalItems = totalItems;
         Items = items;
      }
      public int TotalItems { get; set; }
      public IEnumerable<T> Items { get; set; }
   }
}