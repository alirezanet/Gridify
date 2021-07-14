using System;
using System.Linq;
using System.Linq.Expressions;
using Gridify.Syntax;

namespace Gridify
{
   public partial class GridifyQuery : IGridifyQuery
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

      public Expression<Func<T, bool>> GetFilteringExpression<T>(IGridifyMapper<T> mapper = null)
      {
         if (string.IsNullOrWhiteSpace(Filter))
            throw new GridifyQueryException("Filter is not defined");

         mapper = mapper.FixMapper();

         var syntaxTree = SyntaxTree.Parse(Filter);

         if (syntaxTree.Diagnostics.Any())
            throw new GridifyFilteringException(syntaxTree.Diagnostics.Last()!);

         var queryExpression = ExpressionToQueryConvertor.GenerateQuery(syntaxTree.Root, this, mapper);
         if (queryExpression == null) throw new GridifyQueryException("Can not create expression with current data");
         return queryExpression;
      }
      public Expression<Func<T, object>> GetOrderingExpression<T>(IGridifyMapper<T> mapper = null)
      {
         mapper = mapper.FixMapper();
         if (string.IsNullOrWhiteSpace(SortBy) || !mapper.HasMap(SortBy))
            throw new GridifyQueryException("SortBy is not defined or not Found");
         var expression = mapper.GetExpression(SortBy);
         return expression;
      }
      
   }
}