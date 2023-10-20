using System.Collections.Generic;
using System.Linq;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Gridify.Syntax;

namespace Gridify.Elasticsearch;

public static class GridifyExtensions
{
   public static Query ToElasticsearchQuery<T>(this string? filter, IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(filter))
         return new MatchAllQuery();

      var syntaxTree = SyntaxTree.Parse(filter!, GridifyGlobalConfiguration.CustomOperators.Operators);
      if (syntaxTree.Diagnostics.Any())
         throw new GridifyFilteringException(syntaxTree.Diagnostics.Last());

      mapper ??= mapper.FixMapper(syntaxTree);

      var queryExpression = new ElasticsearchQueryBuilder<T>(mapper).Build(syntaxTree.Root);
      return queryExpression;
   }

   public static ICollection<SortOptions> ToElasticsearchSortOptions<T>(this string? ordering, IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(ordering))
         return new List<SortOptions>();

      var sortOptions = new ElasticsearchSortOptionsBuilder<T>(mapper).Build(ordering!);
      return sortOptions;
   }

   public static SearchRequestDescriptor<T> ApplyFiltering<T>(
      this SearchRequestDescriptor<T> descriptor, string? filter, IGridifyMapper<T>? mapper = null)
   {
      var query = filter.ToElasticsearchQuery(mapper);
      descriptor.Query(query);
      return descriptor;
   }

   public static SearchRequestDescriptor<T> ApplyFiltering<T>(
      this SearchRequestDescriptor<T> descriptor, IGridifyQuery gridifyQuery, IGridifyMapper<T>? mapper = null)
   {
      return descriptor.ApplyFiltering(gridifyQuery.Filter, mapper);
   }

   public static SearchRequestDescriptor<T> ApplyOrdering<T>(
      this SearchRequestDescriptor<T> descriptor, string? ordering, IGridifyMapper<T>? mapper = null)
   {
      var sortOptions = ordering.ToElasticsearchSortOptions(mapper);
      descriptor.Sort(sortOptions);
      return descriptor;
   }

   public static SearchRequestDescriptor<T> ApplyOrdering<T>(
      this SearchRequestDescriptor<T> descriptor, IGridifyQuery gridifyQuery, IGridifyMapper<T>? mapper = null)
   {
      return descriptor.ApplyOrdering(gridifyQuery.OrderBy, mapper);
   }

   public static SearchRequestDescriptor<T> ApplyPaging<T>(
      this SearchRequestDescriptor<T> descriptor, int page, int pageSize)
   {
      var gridifyQuery = new GridifyQuery { Page = page, PageSize = pageSize };
      return descriptor.ApplyPaging((IGridifyPagination)gridifyQuery);
   }

   public static SearchRequestDescriptor<T> ApplyPaging<T>(
      this SearchRequestDescriptor<T> descriptor, IGridifyQuery gridifyQuery)
   {
      return descriptor.ApplyPaging((IGridifyPagination)gridifyQuery);
   }

   public static SearchRequestDescriptor<T> ApplyPaging<T>(
      this SearchRequestDescriptor<T> descriptor, IGridifyPagination gridifyPagination)
   {
      gridifyPagination.FixPagingData();
      return descriptor
         .From((gridifyPagination.Page - 1) * gridifyPagination.PageSize)
         .Size(gridifyPagination.PageSize);
   }

   public static SearchRequestDescriptor<T> ApplyFilteringAndOrdering<T>(
      this SearchRequestDescriptor<T> descriptor, string? filter, string? ordering, IGridifyMapper<T>? mapper = null)
   {
      return descriptor
         .ApplyFiltering(filter, mapper)
         .ApplyOrdering(ordering, mapper);
   }

   public static SearchRequestDescriptor<T> ApplyFilteringAndOrdering<T>(
      this SearchRequestDescriptor<T> descriptor, IGridifyQuery gridifyQuery, IGridifyMapper<T>? mapper = null)
   {
      return descriptor.ApplyFilteringAndOrdering(gridifyQuery.Filter, gridifyQuery.OrderBy, mapper);
   }

   public static SearchRequestDescriptor<T> ApplyFilteringOrderingPaging<T>(
      this SearchRequestDescriptor<T> descriptor, string? filter, string? ordering, int page, int pageSize, IGridifyMapper<T>? mapper = null)
   {
      return descriptor
         .ApplyFilteringAndOrdering(filter, ordering, mapper)
         .ApplyPaging(page, pageSize);
   }

   public static SearchRequestDescriptor<T> ApplyFilteringOrderingPaging<T>(
      this SearchRequestDescriptor<T> descriptor, IGridifyQuery gridifyQuery, IGridifyMapper<T>? mapper = null)
   {
      return descriptor
         .ApplyFilteringAndOrdering(gridifyQuery.Filter, gridifyQuery.OrderBy, mapper)
         .ApplyPaging(gridifyQuery.Page, gridifyQuery.PageSize);
   }

   public static SearchRequestDescriptor<T> ApplyOrderingAndPaging<T>(
      this SearchRequestDescriptor<T> descriptor, string? ordering, int page, int pageSize, IGridifyMapper<T>? mapper = null)
   {
      return descriptor
         .ApplyOrdering(ordering, mapper)
         .ApplyPaging(page, pageSize);
   }

   public static SearchRequestDescriptor<T> ApplyOrderingAndPaging<T>(
      this SearchRequestDescriptor<T> descriptor, IGridifyQuery gridifyQuery, IGridifyMapper<T>? mapper = null)
   {
      return descriptor
         .ApplyOrdering(gridifyQuery.OrderBy, mapper)
         .ApplyPaging(gridifyQuery.Page, gridifyQuery.PageSize);
   }
}
