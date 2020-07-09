using System.Linq;

namespace Gridify
{
   public class QueryablePaging<T> {
      public int TotalItems { get; set; }
      public IQueryable<T> Query { get; set; }
   }
}