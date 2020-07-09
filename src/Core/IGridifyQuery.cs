using System.Collections.Generic;

namespace Gridify {
   public interface IGridifyQuery {
      string SortBy { get; set; }
      bool IsSortAsc { get; set; }
      short Page { get; set; }
      int PageSize { get; set; }
      string Filter { get; set; }
   }
}