using System.Linq;

namespace Gridify
{
   public class QueryablePaging<T>
   {
      public QueryablePaging()
      {
      }
      
      public QueryablePaging(int totalItems, IQueryable<T> query)
      {
         TotalItems = totalItems;
         Query = query;
      }

      public int TotalItems { get; set; }
      public IQueryable<T> Query { get; set; }
   }
}