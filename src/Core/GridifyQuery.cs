namespace Gridify
{
   public class GridifyQuery : IGridifyQuery {
      public string SortBy { get; set; }
      public bool IsSortAsc { get; set; }
      public short Page { get; set; }
      public int PageSize { get; set; }
      public string Filter { get; set; }
   }
}