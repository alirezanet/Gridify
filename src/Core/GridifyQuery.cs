namespace Gridify
{
   public class GridifyQuery : IGridifyQuery
   {
      public GridifyQuery()
      {
      }

      public GridifyQuery(string sortBy = default, bool isSortAsc = default, short page = default, int pageSize = default, string filter = default)
      {
         SortBy = sortBy;
         IsSortAsc = isSortAsc;
         Page = page;
         PageSize = pageSize;
         Filter = filter;
      }

      public string SortBy { get; set; }
      public bool IsSortAsc { get; set; }
      public short Page { get; set; }
      public int PageSize { get; set; }
      public string Filter { get; set; }
   }
}