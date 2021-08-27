namespace Gridify
{
   public class GridifyQuery : IGridifyQuery
   {
      public GridifyQuery()
      {
      }
      public GridifyQuery(short page, int pageSize, string filter, string? orderBy = null)
      {
         Page = page;
         PageSize = pageSize;
         OrderBy = orderBy;
         Filter = filter;
      }

      public short Page { get; set; }
      public int PageSize { get; set; }
      public string? OrderBy { get; set; }
      public string? Filter { get; set; }
   }

  
}