using System;
using System.Collections.Generic;
using Elastic.Clients.Elasticsearch;
using Gridify.Builder;
using Gridify.Syntax;

namespace Gridify.Elasticsearch;

internal class ElasticsearchSortOptionsBuilder<T> : BaseSortingQueryBuilder<ICollection<SortOptions>, T>
{
   internal ElasticsearchSortOptionsBuilder(IGridifyMapper<T>? mapper = null) : base(mapper)
   {
   }

   internal ICollection<SortOptions> Build(string ordering)
   {
      return ProcessOrdering(new List<SortOptions>(), ordering, false);
   }

   protected override ICollection<SortOptions> ApplySorting(ICollection<SortOptions> sortOptions, ParsedOrdering ordering)
   {
      if (ordering.OrderingType != OrderingType.Normal)
      {
         throw new NotSupportedException($"Gridify.Elasticsearch does not support '{ordering.OrderingType}' ordering");
      }

      var propExpression = Mapper!.GetExpression(ordering.MemberName);
      var isStringValue = propExpression.GetRealType() == typeof(string);
      var fieldName = propExpression.Body.BuildFieldName(isStringValue, Mapper);

      var sortOption = SortOptions.Field(fieldName!, new FieldSort { Order = ordering.IsAscending ? SortOrder.Asc : SortOrder.Desc });

      sortOptions.Add(sortOption);
      return sortOptions;
   }

   protected override ICollection<SortOptions> ApplyAnotherSorting(ICollection<SortOptions> sortOptions, ParsedOrdering ordering)
   {
      return ApplySorting(sortOptions, ordering);
   }
}
