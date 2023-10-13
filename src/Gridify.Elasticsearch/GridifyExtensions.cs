using System;
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

      var syntaxTree = SyntaxTree.Parse(filter, GridifyGlobalConfiguration.CustomOperators.Operators);
      if (syntaxTree.Diagnostics.Any())
         throw new GridifyFilteringException(syntaxTree.Diagnostics.Last());

      mapper ??= BuildMapperWithNestedProperties<T>(syntaxTree);

      var (queryExpression, _) = ExpressionToQueryConvertor.GenerateQuery(syntaxTree.Root, mapper);
      return queryExpression;
   }

   public static ICollection<SortOptions> ToElasticsearchSortOptions<T>(this string? ordering, IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(ordering))
         return new List<SortOptions>();

      var sortOptions = ProcessOrdering(ordering, mapper);
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

   private static ICollection<SortOptions> ProcessOrdering<T>(string orderings, IGridifyMapper<T>? mapper)
   {
      var orders = orderings.ParseOrderings().ToList();

      if (mapper is null)
      {
         mapper = new GridifyMapper<T>();
         foreach (var order in orders)
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

      var sortOptions = new List<SortOptions>();
      foreach (var order in orders)
      {
         if (!mapper.HasMap(order.MemberName))
         {
            // skip if there is no mappings available
            if (mapper.Configuration.IgnoreNotMappedFields)
               continue;

            throw new GridifyMapperException($"Mapping '{order.MemberName}' not found");
         }

         var field = mapper.GetExpression(order.MemberName).Body.ToPropertyPath();
         field = mapper.GetExpression(order.MemberName).GetRealType() == typeof(string)
            ? $"{field}.keyword"
            : field;

         var sortOption = SortOptions.Field(
            field,
            new FieldSort { Order = order.IsAscending ? SortOrder.Asc : SortOrder.Desc });

         sortOptions.Add(sortOption);
      }

      return sortOptions;
   }
}
