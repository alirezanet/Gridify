using System.Linq;
using System.Threading;
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
         var (count, queryable) = await query.GridifyQueryableAsync(gridifyQuery, mapper);
         return new Paging<T>(count, await queryable.ToListAsync());
      }

      public async static Task<QueryablePaging<T>> GridifyQueryableAsync<T>(this IQueryable<T> query, IGridifyQuery gridifyQuery,
         IGridifyMapper<T> mapper, CancellationToken token)
      {
         query = query.ApplyFiltering(gridifyQuery, mapper);
         var count = await query.CountAsync(token);
         query = query.ApplyOrdering(gridifyQuery, mapper);
         query = query.ApplyPaging(gridifyQuery);
         return new QueryablePaging<T>(count, query);
      }

      public static async Task<Paging<T>> GridifyAsync<T>(this IQueryable<T> query, IGridifyQuery gridifyQuery, CancellationToken token,
         IGridifyMapper<T> mapper = null)
      {
         var (count, queryable) = await query.GridifyQueryableAsync(gridifyQuery, mapper, token);
         return new Paging<T>(count, await queryable.ToListAsync(token));
      }

      #endregion
   }
}
