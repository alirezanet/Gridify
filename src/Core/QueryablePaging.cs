using System.Linq;

namespace Gridify
{
   public class QueryablePaging<T>
   {
      public QueryablePaging(int count, IQueryable<T> query)
      {
         Count = count;
         Query = query;
      }

      public int Count { get; }
      public IQueryable<T> Query { get; }
   }
}