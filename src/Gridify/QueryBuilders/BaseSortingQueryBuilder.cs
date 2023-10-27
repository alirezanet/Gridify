using System;
using System.Collections.Generic;
using System.Linq;
using Gridify.Syntax;

namespace Gridify.QueryBuilders;

internal abstract class BaseSortingQueryBuilder<TSortingQuery, T>
{
   protected IGridifyMapper<T>? mapper;

   protected BaseSortingQueryBuilder(IGridifyMapper<T>? mapper = null)
   {
      this.mapper = mapper;
   }

   protected abstract TSortingQuery ApplySorting(TSortingQuery query, ParsedOrdering ordering);

   protected abstract TSortingQuery ApplyAnotherSorting(TSortingQuery query, ParsedOrdering ordering);

   internal TSortingQuery ProcessOrdering(TSortingQuery query, string orderings, bool startWithThenBy)
   {
      var isFirst = !startWithThenBy;
      var orders = orderings.ParseOrderings().ToList();
      mapper ??= BuildMapper(orders);

      foreach (var order in orders)
      {
         if (!mapper.HasMap(order.MemberName))
         {
            // skip if there is no mappings available
            if (mapper.Configuration.IgnoreNotMappedFields)
               continue;

            throw new GridifyMapperException($"Mapping '{order.MemberName}' not found");
         }

         if (isFirst)
         {
            query = ApplySorting(query, order);
            isFirst = false;
         }
         else
         {
            query = ApplyAnotherSorting(query, order);
         }
      }

      return query;
   }

   private static GridifyMapper<T> BuildMapper(List<ParsedOrdering> orderings)
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
