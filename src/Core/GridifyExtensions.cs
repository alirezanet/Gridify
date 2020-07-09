using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace Gridify {
   public static partial class GridifyExtensions {

      #region "Static Shared"
      public static int DefaultPageSize = 10;
      #endregion
      #region "Private"

      /// <summary>
      /// Set default <c>Page<c/> number and <c>PageSize<c/> if its not already set in gridifyQuery
      /// </summary>
      /// <param name="gridifyQuery">query and paging configuration</param>
      /// <returns>returns a gridifyQuery with valid PageSize and Page</returns>
      private static IGridifyQuery FixPagingData (this IGridifyQuery gridifyQuery) {
         // set default for page number
         if (gridifyQuery.Page <= 0)
            gridifyQuery.Page = 1;

         // set default for PageSize
         if (gridifyQuery.PageSize <= 0)
            gridifyQuery.PageSize = DefaultPageSize;

         return gridifyQuery;
      }

      private static Expression<Func<T, bool>> GetExpressionFromCondition<T> (string condition, IGridifyQuery gridifyQuery, GridifyMapper<T> mapper) {
         try {
            string[] op = { "==", "!=", "=*", "!*", ">>", "<<", ">=", "<=" };
            var maps = ParseFilter (condition, op);

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
         var conditions = filter.Split (',', '|').ToList ();
         conditions.ForEach (c => {
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

      private static (string Left, string Operation, string Right) ? ParseFilter (string filter, string[] operationList) {
         try {
            string[] map = filter.Split (operationList, StringSplitOptions.None);
            string currentOp = filter.Substring (map[0].Length, 2);
            return (map[0], currentOp, map[1]);
         } catch {
            return null;
         }
      }
      #endregion

      #region "Public"

      /// <summary>
      /// create and return a default <c>GridifyMapper</c>
      /// </summary>
      /// <typeparam name="T">type to set mappings</typeparam>
      /// <returns>returns an auto generated <c>GridifyMapper</c></returns>
      public static GridifyMapper<T> GetDefaultMapper<T> () => new GridifyMapper<T> ().GenerateMappings ();

      /// <summary>
      /// if given mapper was null this function creates default generated mapper
      /// </summary>
      /// <param name="mapper">a <c>GridifyMapper<c/> that can be null</param>
      /// <typeparam name="T">type to set mappings</typeparam>
      /// <returns>return back mapper or new generated mapper if it was null</returns>
      public static GridifyMapper<T> FixMapper<T> (this GridifyMapper<T> mapper) => mapper != null ? mapper : GetDefaultMapper<T> ();

      /// <summary>
      /// adds Filtering,Ordering And Paging to the query
      /// </summary>
      /// <param name="query">the original(target) queryable object</param>
      /// <param name="gridifyQuery">the configuration to apply paging, filtering and ordering</param>
      /// <param name="mapper">this is an optional parameter to apply filtering and ordering using a custom mapping configuration</param>
      /// <typeparam name="T">type of target entity</typeparam>
      /// <returns>returns user query after applying filtering, ordering and paging </returns>
      public static IQueryable<T> ApplyEverything<T> (this IQueryable<T> query, IGridifyQuery gridifyQuery, GridifyMapper<T> mapper = null) {
         if (gridifyQuery == null) return query;
         mapper = mapper.FixMapper ();

         query = query.ApplyFiltering (gridifyQuery, mapper);
         query = query.ApplyOrdering (gridifyQuery, mapper);
         query = query.ApplyPaging (gridifyQuery);
         return query;
      }

      /// <summary>
      /// adds Ordering to the query
      /// </summary>
      /// <param name="query">the original(target) queryable object</param>
      /// <param name="gridifyQuery">the configuration to apply ordering</param>
      /// <param name="mapper">this is an optional parameter to apply ordering using a custom mapping configuration</param>
      /// <typeparam name="T">type of target entity</typeparam>
      /// <returns>returns user query after applying Ordering </returns>
      public static IQueryable<T> ApplyOrdering<T> (this IQueryable<T> query, IGridifyQuery gridifyQuery, GridifyMapper<T> mapper = null) {
         if (gridifyQuery == null) return query;
         mapper = mapper.FixMapper ();
         if (String.IsNullOrWhiteSpace (gridifyQuery.SortBy) || !mapper.Mappings.ContainsKey (gridifyQuery.SortBy))
            return query;
         if (gridifyQuery.IsSortAsc)
            return query.OrderBy (mapper.Mappings[gridifyQuery.SortBy]);
         else
            return query.OrderByDescending (mapper.Mappings[gridifyQuery.SortBy]);
      }

      /// <summary>
      /// adds Ordering to the query
      /// </summary>
      /// <param name="query">the original(target) queryable object</param>
      /// <param name="gridifyQuery">the configuration to apply ordering</param>
      /// <param name="groupOrder">select group member for ordering</param>  // need to be more specific
      /// <param name="mapper">this is an optional parameter to apply ordering using a custom mapping configuration</param>
      /// <typeparam name="T">type of target entity</typeparam>
      /// <returns>returns user query after applying Ordering </returns>
      public static IQueryable<T> ApplyOrdering<T, TKey> (this IQueryable<T> query, IGridifyQuery gridifyQuery, Expression<Func<T, TKey>> groupOrder, GridifyMapper<T> mapper = null) {
         if (gridifyQuery == null) return query;
         mapper = mapper.FixMapper ();
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

         mapper = mapper.FixMapper ();

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
         mapper = mapper.FixMapper ();
         query = query.ApplyOrdering (gridifyQuery, mapper);
         query = query.ApplyPaging (gridifyQuery);
         return query;
      }

      public static QueryablePaging<T> GridifyQueryable<T> (this IQueryable<T> query, IGridifyQuery gridifyQuery, GridifyMapper<T> mapper = null) {
         mapper = mapper.FixMapper ();
         query = query.ApplyFiltering (gridifyQuery, mapper);
         var count = query.Count ();
         query = query.ApplyOrdering (gridifyQuery, mapper);
         query = query.ApplyPaging (gridifyQuery);
         return new QueryablePaging<T> () { TotalItems = count, Query = query };
      }

      /// <summary>
      /// gets a query or collection,
      /// adds filtering, ordering and paging 
      /// loads filtered and sorted data
      /// return pagination ready result 
      /// </summary>
      /// <param name="query">the original(target) queryable object</param>
      /// <param name="gridifyQuery">the configuration to apply paging, filtering and ordering</param>
      /// <param name="mapper">this is an optional parameter to apply filtering and ordering using a custom mapping configuration</param>
      /// <typeparam name="T">type of target entity</typeparam>
      /// <returns>returns a loaded <c>Paging<T><c/> after applying filtering, ordering and paging </returns>
      /// <returns></returns>
      public static Paging<T> Gridify<T> (this IQueryable<T> query, IGridifyQuery gridifyQuery, GridifyMapper<T> mapper = null) {
         mapper = mapper.FixMapper ();
         var res = query.GridifyQueryable (gridifyQuery, mapper);
         return new Paging<T> () { Items = res.Query.ToList (), TotalItems = res.TotalItems };
      }

      #endregion    
   }

}