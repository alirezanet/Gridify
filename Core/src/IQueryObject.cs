using System.Collections.Generic;

namespace TuxTeam.Gridify
{
    public interface IQueryObject
    {
        string SortBy { get; set; }
        bool IsSortAsc { get; set; }
        short Page { get; set; }
        int PageSize { get; set; }
        string Filter { get; set; }    
        (string Left, string Operation, string Right)? ParseFilter(string filter, string[] operationList);
    }
}
