using System;
using System.Collections.Generic;
using System.Linq;
using Gridify.Syntax;

namespace Gridify.Builder;

public abstract class BaseSortingQueryBuilder<TSortingQuery, T>(IGridifyMapper<T>? mapper = null)
{
   protected IGridifyMapper<T>? Mapper = mapper;

   protected abstract TSortingQuery ApplySorting(TSortingQuery query, ParsedOrdering ordering);

   protected abstract TSortingQuery ApplyAnotherSorting(TSortingQuery query, ParsedOrdering ordering);

   public TSortingQuery ProcessOrdering(TSortingQuery query, string orderings, bool startWithThenBy)
   {
      var isFirst = !startWithThenBy;
      var orders = orderings.ParseOrderings().ToList();
      Mapper ??= BuildMapper(orders);

      foreach (var order in orders)
      {
         if (!Mapper.HasMap(order.MemberName))
         {
            // skip if there is no mappings available
            if (Mapper.Configuration.IgnoreNotMappedFields)
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
