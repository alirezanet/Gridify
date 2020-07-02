using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TuxTeam.Gridify.EntityFramework
{
    public static partial class GridifyExtensions {

      #region "EntityFramework Integration"
      public async static Task < QueryablePaging<T> > ApplyEverythingWithCountAsync<T> (this IQueryable<T> query, IGridifyQuery gridifyQuery, GridifyMapper<T> mapper) {
         query = query.ApplyFiltering (gridifyQuery, mapper);
         var count = await query.CountAsync ();
         query = query.ApplyOrdering (gridifyQuery, mapper);
         query = query.ApplyPaging (gridifyQuery);
         return new QueryablePaging<T>(){ TotalItems= count, query= query };
      }
      public async static Task<Paging<T>> GridifyAsync<T> (this IQueryable<T> query, IGridifyQuery gridifyQuery, GridifyMapper<T> mapper = null) {
         mapper = mapper.FixMapper ();
         var res = await query.ApplyEverythingWithCountAsync (gridifyQuery, mapper);
         return new Paging<T> () { Items = await res.gridifyQuery.ToListAsync (), TotalItems = res.Count };
      }
      #endregion
   }
}