using System.Collections.Generic;
using System.Linq;

namespace TuxTeam.Gridify
{
    public class QueryablePaging<T>
    {
        public int TotalItems { get; set; }
        public IQueryable<T> Query { get; set; }
    }
}
