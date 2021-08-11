using System.Collections.Generic;

namespace Gridify
{
   public interface IGridifyQuery : IGridifyPagination, IGridifyFiltering, IGridifyOrdering
   {
   }
}