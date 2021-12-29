namespace Gridify;

public class GridifyQuery : IGridifyQuery
{
   public GridifyQuery()
   {
   }
   public GridifyQuery(int page, int pageSize, string filter, string? orderBy = null, string? select = null)
   {
      Page = page;
      PageSize = pageSize;
      OrderBy = orderBy;
      Filter = filter;
      Select = select;
   }

   public int Page { get; set; }
   public int PageSize { get; set; }
   public string? OrderBy { get; set; }
   public string? Filter { get; set; }
   public string? Select { get; set; }
}