using System.Collections.Generic;
using System.Linq;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Gridify.Syntax;

namespace Gridify.Elasticsearch;

public static class GridifyExtensions
{
   /// <summary>
   /// Converts a Gridify filter string to an Elasticsearch DSL <see cref="Query"/> object.
   /// </summary>
   /// <param name="filtering">Gridify filter</param>
   /// <param name="mapper">Gridify mapper</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch DSL <see cref="Query"/> object</returns>
   /// <exception cref="GridifyFilteringException">Throws when the filter string is invalid</exception>
   public static Query ToElasticsearchQuery<T>(this IGridifyFiltering? filtering, IGridifyMapper<T>? mapper = null)
   {
      return ToElasticsearchQuery(filtering?.Filter, mapper);
   }

   /// <summary>
   /// Converts a Gridify sorting string to an Elasticsearch <see cref="ICollection&lt;SortOptions&gt;"/> sort options.
   /// </summary>
   /// <param name="ordering">Gridify ordering string</param>
   /// <param name="mapper">Gridify mapper</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch <see cref="ICollection&lt;SortOptions&gt;"/> sort options</returns>
   public static ICollection<SortOptions> ToElasticsearchSortOptions<T>(this IGridifyOrdering? ordering, IGridifyMapper<T>? mapper = null)
   {
      return ToElasticsearchSortOptions(ordering?.OrderBy, mapper);
   }

   /// <summary>
   /// Applies Gridify filter string to an Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor.
   /// </summary>
   /// <param name="descriptor">Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</param>
   /// <param name="filter">Gridify filter string</param>
   /// <param name="mapper">Gridify mapper</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</returns>
   public static SearchRequestDescriptor<T> ApplyFiltering<T>(
      this SearchRequestDescriptor<T> descriptor, string? filter, IGridifyMapper<T>? mapper = null)
   {
      var query = filter.ToElasticsearchQuery(mapper);
      descriptor.Query(query);
      return descriptor;
   }

   /// <summary>
   /// Applies <see cref="IGridifyQuery.Filter"/> to an Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor.
   /// </summary>
   /// <param name="descriptor">Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</param>
   /// <param name="gridifyQuery">Gridify query</param>
   /// <param name="mapper">Gridify mapper</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</returns>
   public static SearchRequestDescriptor<T> ApplyFiltering<T>(
      this SearchRequestDescriptor<T> descriptor, IGridifyQuery gridifyQuery, IGridifyMapper<T>? mapper = null)
   {
      return descriptor.ApplyFiltering(gridifyQuery.Filter, mapper);
   }

   /// <summary>
   /// Applies Gridify ordering string to an Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor.
   /// </summary>
   /// <param name="descriptor">Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</param>
   /// <param name="ordering">Gridify ordering string</param>
   /// <param name="mapper">Gridify mapper</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</returns>
   public static SearchRequestDescriptor<T> ApplyOrdering<T>(
      this SearchRequestDescriptor<T> descriptor, string? ordering, IGridifyMapper<T>? mapper = null)
   {
      var sortOptions = ordering.ToElasticsearchSortOptions(mapper);
      descriptor.Sort(sortOptions);
      return descriptor;
   }

   /// <summary>
   /// Applies ordering to an Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor.
   /// </summary>
   /// <param name="descriptor">Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</param>
   /// <param name="gridifyQuery">Gridify query</param>
   /// <param name="mapper">Gridify mapper</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</returns>
   public static SearchRequestDescriptor<T> ApplyOrdering<T>(
      this SearchRequestDescriptor<T> descriptor, IGridifyQuery gridifyQuery, IGridifyMapper<T>? mapper = null)
   {
      return descriptor.ApplyOrdering(gridifyQuery.OrderBy, mapper);
   }

   /// <summary>
   /// Applies paging to an Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor.
   /// Fixes paging data if it is invalid.
   /// </summary>
   /// <param name="descriptor">Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</param>
   /// <param name="page">Page number</param>
   /// <param name="pageSize">Page size</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</returns>
   public static SearchRequestDescriptor<T> ApplyPaging<T>(
      this SearchRequestDescriptor<T> descriptor, int page, int pageSize)
   {
      var gridifyQuery = new GridifyQuery { Page = page, PageSize = pageSize };
      return descriptor.ApplyPaging((IGridifyPagination)gridifyQuery);
   }

   /// <summary>
   /// Applies paging to an Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor.
   /// Fixes paging data if it is invalid.
   /// </summary>
   /// <param name="descriptor">Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</param>
   /// <param name="gridifyQuery">Gridify query</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</returns>
   public static SearchRequestDescriptor<T> ApplyPaging<T>(
      this SearchRequestDescriptor<T> descriptor, IGridifyQuery gridifyQuery)
   {
      return descriptor.ApplyPaging((IGridifyPagination)gridifyQuery);
   }

   /// <summary>
   /// Applies paging to an Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor.
   /// Fixes paging data if it is invalid.
   /// </summary>
   /// <param name="descriptor">Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</param>
   /// <param name="gridifyPagination">Gridify pagination</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</returns>
   public static SearchRequestDescriptor<T> ApplyPaging<T>(
      this SearchRequestDescriptor<T> descriptor, IGridifyPagination gridifyPagination)
   {
      gridifyPagination.FixPagingData();
      return descriptor
         .From((gridifyPagination.Page - 1) * gridifyPagination.PageSize)
         .Size(gridifyPagination.PageSize);
   }

   /// <summary>
   /// Applies filtering, ordering and paging to an Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor.
   /// </summary>
   /// <param name="descriptor">Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</param>
   /// <param name="filter">Gridify filter string</param>
   /// <param name="ordering">Gridify ordering string</param>
   /// <param name="mapper">Gridify mapper</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</returns>
   public static SearchRequestDescriptor<T> ApplyFilteringAndOrdering<T>(
      this SearchRequestDescriptor<T> descriptor, string? filter, string? ordering, IGridifyMapper<T>? mapper = null)
   {
      return descriptor
         .ApplyFiltering(filter, mapper)
         .ApplyOrdering(ordering, mapper);
   }

   /// <summary>
   /// Applies filtering, ordering and paging to an Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor.
   /// </summary>
   /// <param name="descriptor">Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</param>
   /// <param name="gridifyQuery">Gridify query</param>
   /// <param name="mapper">Gridify mapper</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</returns>
   public static SearchRequestDescriptor<T> ApplyFilteringAndOrdering<T>(
      this SearchRequestDescriptor<T> descriptor, IGridifyQuery gridifyQuery, IGridifyMapper<T>? mapper = null)
   {
      return descriptor.ApplyFilteringAndOrdering(gridifyQuery.Filter, gridifyQuery.OrderBy, mapper);
   }

   /// <summary>
   /// Applies filtering, ordering and paging to an Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor.
   /// </summary>
   /// <param name="descriptor">Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</param>
   /// <param name="filter">Gridify filter string</param>
   /// <param name="ordering">Gridify ordering string</param>
   /// <param name="page">Page number</param>
   /// <param name="pageSize">Page size</param>
   /// <param name="mapper">Gridify mapper</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</returns>
   public static SearchRequestDescriptor<T> ApplyFilteringOrderingPaging<T>(
      this SearchRequestDescriptor<T> descriptor, string? filter, string? ordering, int page, int pageSize, IGridifyMapper<T>? mapper = null)
   {
      return descriptor
         .ApplyFilteringAndOrdering(filter, ordering, mapper)
         .ApplyPaging(page, pageSize);
   }

   /// <summary>
   /// Applies filtering, ordering and paging to an Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor.
   /// </summary>
   /// <param name="descriptor">Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</param>
   /// <param name="gridifyQuery">Gridify query</param>
   /// <param name="mapper">Gridify mapper</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</returns>
   public static SearchRequestDescriptor<T> ApplyFilteringOrderingPaging<T>(
      this SearchRequestDescriptor<T> descriptor, IGridifyQuery gridifyQuery, IGridifyMapper<T>? mapper = null)
   {
      return descriptor
         .ApplyFilteringAndOrdering(gridifyQuery.Filter, gridifyQuery.OrderBy, mapper)
         .ApplyPaging(gridifyQuery.Page, gridifyQuery.PageSize);
   }

   /// <summary>
   /// Applies ordering and paging to an Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor.
   /// </summary>
   /// <param name="descriptor">Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</param>
   /// <param name="ordering">Gridify ordering string</param>
   /// <param name="page">Page number</param>
   /// <param name="pageSize">Page size</param>
   /// <param name="mapper">Gridify mapper</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</returns>
   public static SearchRequestDescriptor<T> ApplyOrderingAndPaging<T>(
      this SearchRequestDescriptor<T> descriptor, string? ordering, int page, int pageSize, IGridifyMapper<T>? mapper = null)
   {
      return descriptor
         .ApplyOrdering(ordering, mapper)
         .ApplyPaging(page, pageSize);
   }

   /// <summary>
   /// Applies ordering and paging to an Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor.
   /// </summary>
   /// <param name="descriptor">Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</param>
   /// <param name="gridifyQuery">Gridify query</param>
   /// <param name="mapper">Gridify mapper</param>
   /// <typeparam name="T">Entity type</typeparam>
   /// <returns>Elasticsearch <see cref="SearchRequestDescriptor&lt;T&gt;"/> descriptor</returns>
   public static SearchRequestDescriptor<T> ApplyOrderingAndPaging<T>(
      this SearchRequestDescriptor<T> descriptor, IGridifyQuery gridifyQuery, IGridifyMapper<T>? mapper = null)
   {
      return descriptor
         .ApplyOrdering(gridifyQuery.OrderBy, mapper)
         .ApplyPaging(gridifyQuery.Page, gridifyQuery.PageSize);
   }

   private static Query ToElasticsearchQuery<T>(this string? filter, IGridifyMapper<T>? mapper = null)
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

   private static ICollection<SortOptions> ToElasticsearchSortOptions<T>(this string? ordering, IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(ordering))
         return new List<SortOptions>();

      var sortOptions = new ElasticsearchSortOptionsBuilder<T>(mapper).Build(ordering);
      return sortOptions;
   }
}
