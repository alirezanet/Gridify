using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
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

      var syntaxTree = SyntaxTree.Parse(filter, GridifyGlobalConfiguration.CustomOperators.Operators);
      if (syntaxTree.Diagnostics.Any())
         throw new GridifyFilteringException(syntaxTree.Diagnostics.Last());

      mapper ??= BuildMapperWithNestedProperties<T>(syntaxTree);

      var queryExpression = ToElasticsearchConverter.GenerateQuery(syntaxTree.Root, mapper);
      return queryExpression;
   }

   public static ICollection<SortOptions> ToElasticsearchSortOptions<T>(this string? ordering, IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(ordering))
         return new List<SortOptions>();

      var orderings = ordering.ParseOrderings().ToList();

      mapper ??= BuildMapperForSorting<T>(orderings);

      var sortOptions = ToElasticsearchConverter.GenerateSortOptions(orderings, mapper);
      return sortOptions;
   }

   private static GridifyMapper<T> BuildMapperWithNestedProperties<T>(SyntaxTree syntaxTree)
   {
      var mapper = new GridifyMapper<T>();
      foreach (var field in syntaxTree.Root.Descendants()
                  .Where(q => q.Kind == SyntaxKind.FieldExpression)
                  .Cast<FieldExpressionSyntax>())
         try
         {
            mapper.AddMap(field.FieldToken.Text);
         }
         catch (Exception)
         {
            if (!mapper.Configuration.IgnoreNotMappedFields)
               throw new GridifyMapperException($"Property '{field.FieldToken.Text}' not found.");
         }

      return mapper;
   }

   private static IEnumerable<SyntaxNode> Descendants(this SyntaxNode root)
   {
      var nodes = new Stack<SyntaxNode>(new[] { root });
      while (nodes.Any())
      {
         var node = nodes.Pop();
         yield return node;
         foreach (var n in node.GetChildren()) nodes.Push(n);
      }
   }

   private static GridifyMapper<T> BuildMapperForSorting<T>(List<ParsedOrdering> orderings)
   {
      var mapper = new GridifyMapper<T>();
      foreach (var order in orderings)
      {
         try
         {
            mapper.AddMap(order.MemberName);
         }
         catch (Exception)
         {
            if (!mapper.Configuration.IgnoreNotMappedFields)
               throw new GridifyMapperException($"Mapping '{order.MemberName}' not found");
         }
      }

      return mapper;
   }
}
