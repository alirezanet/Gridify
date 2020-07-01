using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace TuxTeam.Gridify.EntityFramework
{
    public static partial class GridifyExtensions {

      #region "EntityFramework Integration"
      public async static Task < (int Count, IQueryable<T> DataQuery) > ApplyEverythingWithCountAsync<T> (this IQueryable<T> query, IQueryObject queryObj, QueryColumnMapper<T> columnMapper) {
         query = query.ApplyFiltering (queryObj, columnMapper);
         var count = await query.CountAsync ();
         query = query.ApplyOrdering (queryObj, columnMapper);
         query = query.ApplyPaging (queryObj);
         return (count, query);
      }
      public async static Task<Paging<T>> GridifyAsync<T> (this IQueryable<T> query, IQueryObject queryObj, QueryColumnMapper<T> columnMapper = null) {
         columnMapper = columnMapper.FixColumnMapper ();
         var res = await query.ApplyEverythingWithCountAsync (queryObj, columnMapper);
         return new Paging<T> () { Items = await res.DataQuery.ToListAsync (), TotalItems = res.Count };
      }
      #endregion
   }
}