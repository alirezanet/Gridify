using System;
using System.Linq;
using System.Linq.Expressions;
using Gridify.Syntax;

namespace Gridify
{
   public class GridifyQuery : IGridifyQuery
   {
      public GridifyQuery()
      {
      }
      public GridifyQuery(string orderBy, short page, int pageSize, string filter)
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