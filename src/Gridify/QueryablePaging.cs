using System.Linq;

namespace Gridify;

public class QueryablePaging<T>(int count, IQueryable<T> query)
{
   public void Deconstruct(out int count, out IQueryable<T> query)
   {
      count = Count;
      query = Query;
   }
   public int Count { get; } = count;
   public IQueryable<T> Query { get; } = query;
}
