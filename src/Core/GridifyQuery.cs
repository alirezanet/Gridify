namespace Gridify
{
   public class GridifyQuery : IGridifyQuery
   {
      public GridifyQuery()
      {
      }

      public GridifyQuery(string sortBy, bool isSortAsc, short page, int pageSize, string filter)
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