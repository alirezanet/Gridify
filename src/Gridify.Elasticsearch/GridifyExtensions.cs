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
}
