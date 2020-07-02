using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace TuxTeam.Gridify {
   public static partial class GridifyExtensions {

#region "Static Shared"
   public static short DefaultPage = 1;
   public static int DefaultPageSize = 10;
#endregion
#region "Private"

      private static IGridifyQuery FixPagingData (this IGridifyQuery gridifyQuery) {
         // set default for page number
         if (gridifyQuery.Page <= 0)
            gridifyQuery.Page = DefaultPage;

         // set default for PageSize
         if (gridifyQuery.PageSize <= 0)
            gridifyQuery.PageSize = DefaultPageSize;

         return gridifyQuery;
      }

         private static Expression<Func<T, bool>> GetExpressionFromCondition<T> (string condition, IGridifyQuery gridifyQuery, GridifyMapper<T> mapper) {
         try {
            string[] op = { "==", "!=", "=*", "!*", ">>", "<<", ">=", "<=" };
            var maps = gridifyQuery.ParseFilter (condition, op);

            if (!maps.HasValue)
               return null;

            Expression<Func<T, object>> exp = mapper.Mappings[maps.Value.Left];
            var body = exp.Body;

            // Remove the boxing for value types
            if (body.NodeType == ExpressionType.Convert) {
               body = ((UnaryExpression) body).Operand;
            }
            object value = maps.Value.Right;
            if (value != null && body.Type != value.GetType ()) {
               var converter = TypeDescriptor.GetConverter (body.Type);
               value = converter.ConvertFromString (value.ToString ());
               // value = Convert.ChangeType(value, body.Type);
            }
            Expression be = null;
            var ContainsMethod = typeof (string).GetMethod ("Contains", new [] { typeof (string) });
            switch (maps.Value.Operation) {
               case "==":
                  be = Expression.Equal (body, Expression.Constant (value, body.Type));
                  break;
               case "!=":
                  be = Expression.NotEqual (body, Expression.Constant (value, body.Type));
                  break;
               case ">>":
                  be = Expression.GreaterThan (body, Expression.Constant (value, body.Type));
                  break;
               case "<<":
                  be = Expression.LessThan (body, Expression.Constant (value, body.Type));
                  break;
               case ">=":
                  be = Expression.GreaterThanOrEqual (body, Expression.Constant (value, body.Type));
                  break;
               case "<=":
                  be = Expression.LessThanOrEqual (body, Expression.Constant (value, body.Type));
                  break;
               case "=*":
                  be = Expression.Call (body, ContainsMethod, Expression.Constant (value, body.Type));
                  break;
               case "!*":
                  be = Expression.Not (Expression.Call (body, ContainsMethod, Expression.Constant (value, body.Type)));
                  break;
               default:
                  return null;
            }
            return Expression.Lambda<Func<T, bool>> (be, exp.Parameters);
         } catch (Exception) {
            return null; 
         }
      }

      private static List < (string condition, bool IsAnd) > CreateConditions (string filter) {
         var filters = new List < (string condition, bool IsAnd) > ();
         var conds = filter.Split (',', '|').ToList ();
         conds.ForEach (c => {
            var OpIndex = filter.IndexOf (c) + c.Length;
            string nextOp = OpIndex + 1 <= filter.Length ? filter.Substring (OpIndex, 1) : ",";
            filters.Add ((c.Trim (), nextOp != "|"));
         });
         return filters;
      }
      private static List < (string condition, bool IsAnd) > GetComplexFilters (string filter) {
         var complexFilters = new List < (string condition, bool IsAnd) > ();
         var processing = true;
         while (processing) {
            var closePosition = filter.IndexOf (")");
            var condition = filter.Substring (1, closePosition - 1);

            if (closePosition + 1 >= filter.Length) // text parsed
            {
               complexFilters.Add ((condition.Trim (), true));
               processing = false;
            } else {
               var nextOp = filter.Substring (closePosition + 1, 1);
               filter = filter.Substring (closePosition + 2, filter.Length - (closePosition + 2));
               complexFilters.Add ((condition.Trim (), nextOp == ","));
            }
         }
         return complexFilters;
      }

      private static Expression<Func<T, bool>> GetExpressionFromInternalConditions<T> (IGridifyQuery gridifyQuery, GridifyMapper<T> mapper, List < (string condition, bool IsAnd) > conditions) {
         Expression<Func<T, bool>> finalExp = null;
         bool nextOpIsAnd = true;
         conditions.ForEach (c => {
            var exp = GetExpressionFromCondition (c.condition, gridifyQuery, mapper);
            if (exp == null) return;
            if (finalExp == null)
               finalExp = exp;
            else {
               if (nextOpIsAnd)
                  finalExp = PredicateBuilder.And (finalExp, exp);
               else
                  finalExp = PredicateBuilder.Or (finalExp, exp);
            }
            nextOpIsAnd = c.IsAnd;
         });
         return finalExp;
      }
#endregion

#region "Public"
      public static GridifyMapper<T> GetDefaultMapper<T> () => new GridifyMapper<T> ().GenerateMappings ();
      public static GridifyMapper<T> FixMapper<T>(this GridifyMapper<T> mapper) 
         => mapper != null ? mapper : GetDefaultMapper<T>();

      public static IQueryable<T> ApplyEverything<T> (this IQueryable<T> query, IGridifyQuery gridifyQuery, GridifyMapper<T> mapper = null) {
         if (gridifyQuery == null) return query;
         mapper = mapper.FixMapper();

         query = query.ApplyFiltering (gridifyQuery, mapper);
         query = query.ApplyOrdering (gridifyQuery, mapper);
         query = query.ApplyPaging (gridifyQuery);
         return query;
      }

      public static IQueryable<T> ApplyOrdering<T> (this IQueryable<T> query, IGridifyQuery gridifyQuery, GridifyMapper<T> mapper = null) {
         if (gridifyQuery == null) return query;
         mapper = mapper.FixMapper();
         if (String.IsNullOrWhiteSpace (gridifyQuery.SortBy) || !mapper.Mappings.ContainsKey (gridifyQuery.SortBy))
            return query;
         if (gridifyQuery.IsSortAsc)
            return query.OrderBy (mapper.Mappings[gridifyQuery.SortBy]);
         else
            return query.OrderByDescending (mapper.Mappings[gridifyQuery.SortBy]);
      }
      public static IQueryable<T> ApplyOrdering<T, Tkey> (this IQueryable<T> query, IGridifyQuery gridifyQuery, Expression<Func<T, Tkey>> groupOrder, GridifyMapper<T> mapper = null) {
         if (gridifyQuery == null) return query;
         mapper = mapper.FixMapper();
         if (String.IsNullOrWhiteSpace (gridifyQuery.SortBy) || !mapper.Mappings.ContainsKey (gridifyQuery.SortBy))
            return query;
         if (gridifyQuery.IsSortAsc)
            return query.OrderBy (groupOrder).ThenBy (mapper.Mappings[gridifyQuery.SortBy]);
         else
            return query.OrderByDescending (groupOrder).ThenBy (mapper.Mappings[gridifyQuery.SortBy]);
      }

      public static IQueryable<T> ApplyPaging<T> (this IQueryable<T> query, IGridifyQuery gridifyQuery) {
         if (gridifyQuery == null) return query;
         gridifyQuery = gridifyQuery.FixPagingData ();
         return query.Skip ((gridifyQuery.Page - 1) * gridifyQuery.PageSize).Take (gridifyQuery.PageSize);
      }

      public static IQueryable<IGrouping<T2, T>> ApplyPaging<T, T2> (this IQueryable<IGrouping<T2, T>> query, IGridifyQuery gridifyQuery) {
         if (gridifyQuery == null) return query;
         gridifyQuery = gridifyQuery.FixPagingData ();
         return query.Skip ((gridifyQuery.Page - 1) * gridifyQuery.PageSize).Take (gridifyQuery.PageSize);
      }
   
      public static IQueryable<T> ApplyFiltering<T> (this IQueryable<T> query, IGridifyQuery gridifyQuery, GridifyMapper<T> mapper = null) {
         if (gridifyQuery == null) return query;
         if (String.IsNullOrWhiteSpace (gridifyQuery.Filter))
            return query;
         
         mapper = mapper.FixMapper();
         
         var textFilter = gridifyQuery.Filter.Trim ();
         Expression<Func<T, bool>> queryExpression = null;
         if (textFilter.Trim ().StartsWith ("(")) // its complex query
         {
            var complexFilters = GetComplexFilters (textFilter.Trim ());
            Expression<Func<T, bool>> finalExp = null;
            bool nextOpIsAnd = true;
            foreach (var complexFilter in complexFilters) {
               var conditions = CreateConditions (complexFilter.condition);
               var internalExpression = GetExpressionFromInternalConditions (gridifyQuery, mapper, conditions);
               if (finalExp == null)
                  finalExp = internalExpression;
               else {
                  if (nextOpIsAnd)
                     finalExp = PredicateBuilder.And (finalExp, internalExpression);
                  else
                     finalExp = PredicateBuilder.Or (finalExp, internalExpression);
               }
               nextOpIsAnd = complexFilter.IsAnd;
            }
            queryExpression = finalExp;
         } else { // its normal query
            var conditions = CreateConditions (textFilter);
            queryExpression = GetExpressionFromInternalConditions (gridifyQuery, mapper, conditions);
         }
         if (queryExpression != null) query = query.Where (queryExpression);
         return query;
      }

      public static IQueryable<T> ApplyOrderingAndPaging<T> (this IQueryable<T> query, IGridifyQuery gridifyQuery, GridifyMapper<T> mapper = null) {
         mapper = mapper.FixMapper();
         query = query.ApplyOrdering (gridifyQuery, mapper);
         query = query.ApplyPaging (gridifyQuery);
         return query;
      }

      public static QueryablePaging<T> ApplyEverythingWithCount<T> (this IQueryable<T> query, IGridifyQuery gridifyQuery, GridifyMapper<T> mapper = null) {
         mapper=mapper.FixMapper();
         query = query.ApplyFiltering (gridifyQuery, mapper);
         var count = query.Count ();
         query = query.ApplyOrdering (gridifyQuery, mapper);
         query = query.ApplyPaging (gridifyQuery);
         return new QueryablePaging<T>(){TotalItems=count,Query=query};
      }

      public static Paging<T> Gridify<T> (this IQueryable<T> query, IGridifyQuery gridifyQuery, GridifyMapper<T> mapper = null) {
         mapper = mapper.FixMapper();
         var res = query.ApplyEverythingWithCount (gridifyQuery, mapper);
         return new Paging<T> () { Items = res.Query.ToList (), TotalItems = res.TotalItems };
      }

  #endregion    

#region "AutoMapper"
      // AutoMapper Integration (need AutoMapper as a Dependency)
      // public async static Task<Paging<TDestination>> GridifyToAsync<TSource, TDestination> (this IQueryable<TSource> query, IGridifyQuery gridifyQuery, GridifyMapper<TSource> mapper) {
      //    var res = await query.ApplyQueryWithCountAsync (gridifyQuery, mapper);
      //    return new Paging<TDestination> () { Items = await res.gridifyQuery.ProjectTo<TDestination> ().ToListAsync (), TotalItems = res.Count };
      // }
      // public async static Task<Paging<D>> GridifyToAsync<T, D> (this IQueryable<T> query, IGridifyQuery gridifyQuery) 
      //    => await FetchToAsync<T, D> (query, gridifyQuery, GetDefaultMapper<T> ());
   }
#endregion
}
