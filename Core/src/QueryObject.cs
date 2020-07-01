using System;
using System.Collections.Generic;
using System.Linq;

namespace TuxTeam.EzPaging
{
    public class QueryObject : IQueryObject
    {
        public string SortBy { get; set; }
        public bool IsSortAsc { get; set; }
        public short Page { get; set; }
        public int PageSize { get; set; }
        public string Filter { get; set; }

        public (string Left, string Operation, string Right)? ParseFilter(string filter, string[] operationList)
        {
           try
           {
            string[] map = filter.Split(operationList, StringSplitOptions.None);
            string currentOp = filter.Substring(map[0].Length, 2);
            return (map[0], currentOp, map[1]);
           }
           catch
           {
                return null;
           }
        }
    }

}
