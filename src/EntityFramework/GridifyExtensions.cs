using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Gridify.EntityFramework
{
   public static partial class GridifyExtensions
   {
      #region "EntityFramework Integration"

      public async static Task<QueryablePaging<T>> GridifyQueryableAsync<T>(this IQueryable<T> query, IGridifyQuery gridifyQuery,
         IGridifyMapper<T> mapper)
      {
         query = query.ApplyFiltering(gridifyQuery, mapper);
         var count = await query.CountAsync();
         query = query.ApplyOrdering(gridifyQuery, mapper);
         query = query.ApplyPaging(gridifyQuery);
         return new QueryablePaging<T>(count, query);
      }

      public static async Task<Paging<T>> GridifyAsync<T>(this IQueryable<T> query, IGridifyQuery gridifyQuery, IGridifyMapper<T> mapper = null)
      {
         mapper = mapper.FixMapper();
         var res = await query.GridifyQueryableAsync(gridifyQuery, mapper);
         return new Paging<T>(res.Count, await res.Query.ToListAsync());
      }

      #endregion
   }
}