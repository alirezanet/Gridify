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
      public void Deconstruct(out int count, out IQueryable<T> query)
      {
         count = Count;
         query = Query;
      }
      public int Count { get; }
      public IQueryable<T> Query { get; }
   }
}
