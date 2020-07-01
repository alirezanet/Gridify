using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;

namespace TuxTeam.EzPaging {
   public static partial class EzPagingExtensions {

#region "Static Shared"
   public static short DefaultPage = 1;
   public static int DefaultPageSize = 10;
#endregion
#region "Private"

      private static QueryColumnMapper<T> GetDefaultColumnMapper<T> () => new QueryColumnMapper<T> ().GenerateMappings ();
      public static QueryColumnMapper<T> FixColumnMapper<T>(this QueryColumnMapper<T> columnMapper) 
         => columnMapper != null ? columnMapper : GetDefaultColumnMapper<T>();

      private static IQueryObject FixPagingData (this IQueryObject queryObj) {
         // set default for page number
         if (queryObj.Page <= 0)
            queryObj.Page = DefaultPage;

         // set default for PageSize
         if (queryObj.PageSize <= 0)
            queryObj.PageSize = DefaultPageSize;

         return queryObj;
      }

         private static Expression<Func<T, bool>> GetExpressionFromConditon<T> (string condition, IQueryObject queryObj, QueryColumnMapper<T> columnMapper) {
         try {
            string[] op = { "==", "!=", "=*", "!*", ">>", "<<", ">=", "<=" };
            var maps = queryObj.ParseFilter (condition, op);

            if (!maps.HasValue)
               return null;

            Expression<Func<T, object>> exp = columnMapper.Mappings[maps.Value.Left];
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

      private static Expression<Func<T, bool>> GetExpressionFromInternalConditions<T> (IQueryObject queryObj, QueryColumnMapper<T> columnMapper, List < (string condition, bool IsAnd) > conditions) {
         Expression<Func<T, bool>> finalExp = null;
         bool nextOpIsAnd = true;
         conditions.ForEach (c => {
            var exp = GetExpressionFromConditon (c.condition, queryObj, columnMapper);
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

      public static IQueryable<T> ApplyEverything<T> (this IQueryable<T> query, IQueryObject queryObj, QueryColumnMapper<T> columnMapper = null) {
         if (queryObj == null) return query;
         columnMapper = columnMapper.FixColumnMapper();

         query = query.ApplyFiltering (queryObj, columnMapper);
         query = query.ApplyOrdering (queryObj, columnMapper);
         query = query.ApplyPaging (queryObj);
         return query;
      }

      public static IQueryable<T> ApplyOrdering<T> (this IQueryable<T> query, IQueryObject queryObj, QueryColumnMapper<T> columnMapper = null) {
         if (queryObj == null) return query;
         columnMapper = columnMapper.FixColumnMapper();
         if (String.IsNullOrWhiteSpace (queryObj.SortBy) || !columnMapper.Mappings.ContainsKey (queryObj.SortBy))
            return query;
         if (queryObj.IsSortAsc)
            return query.OrderBy (columnMapper.Mappings[queryObj.SortBy]);
         else
            return query.OrderByDescending (columnMapper.Mappings[queryObj.SortBy]);
      }
      public static IQueryable<T> ApplyOrdering<T, Tkey> (this IQueryable<T> query, IQueryObject queryObj, Expression<Func<T, Tkey>> groupOrder, QueryColumnMapper<T> columnMapper = null) {
         if (queryObj == null) return query;
         columnMapper = columnMapper.FixColumnMapper();
         if (String.IsNullOrWhiteSpace (queryObj.SortBy) || !columnMapper.Mappings.ContainsKey (queryObj.SortBy))
            return query;
         if (queryObj.IsSortAsc)
            return query.OrderBy (groupOrder).ThenBy (columnMapper.Mappings[queryObj.SortBy]);
         else
            return query.OrderByDescending (groupOrder).ThenBy (columnMapper.Mappings[queryObj.SortBy]);
      }

      public static IQueryable<T> ApplyPaging<T> (this IQueryable<T> query, IQueryObject queryObj) {
         if (queryObj == null) return query;
         queryObj = queryObj.FixPagingData ();
         return query.Skip ((queryObj.Page - 1) * queryObj.PageSize).Take (queryObj.PageSize);
      }

      public static IQueryable<IGrouping<T2, T>> ApplyPaging<T, T2> (this IQueryable<IGrouping<T2, T>> query, IQueryObject queryObj) {
         if (queryObj == null) return query;
         queryObj = queryObj.FixPagingData ();
         return query.Skip ((queryObj.Page - 1) * queryObj.PageSize).Take (queryObj.PageSize);
      }
   
      public static IQueryable<T> ApplyFiltering<T> (this IQueryable<T> query, IQueryObject queryObj, QueryColumnMapper<T> columnMapper = null) {
         if (queryObj == null) return query;
         if (String.IsNullOrWhiteSpace (queryObj.Filter))
            return query;
         
         columnMapper = columnMapper.FixColumnMapper();
         
         var textFilter = queryObj.Filter.Trim ();
         Expression<Func<T, bool>> queryExpression = null;
         if (textFilter.Trim ().StartsWith ("(")) // its complex query
         {
            var complexFilters = GetComplexFilters (textFilter.Trim ());
            Expression<Func<T, bool>> finalExp = null;
            bool nextOpIsAnd = true;
            foreach (var complexFilter in complexFilters) {
               var conditions = CreateConditions (complexFilter.condition);
               var internalExpression = GetExpressionFromInternalConditions (queryObj, columnMapper, conditions);
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
            queryExpression = GetExpressionFromInternalConditions (queryObj, columnMapper, conditions);
         }
         if (queryExpression != null) query = query.Where (queryExpression);
         return query;
      }

      public static IQueryable<T> ApplyOrderingAndPaging<T> (this IQueryable<T> query, IQueryObject queryObj, QueryColumnMapper<T> columnMapper = null) {
         columnMapper = columnMapper.FixColumnMapper();
         query = query.ApplyOrdering (queryObj, columnMapper);
         query = query.ApplyPaging (queryObj);
         return query;
      }

      public static (int Count, IQueryable<T> DataQuery) ApplyEverythingWithCount<T> (this IQueryable<T> query, IQueryObject queryObj, QueryColumnMapper<T> columnMapper = null) {
         columnMapper=columnMapper.FixColumnMapper();
         query = query.ApplyFiltering (queryObj, columnMapper);
         var count = query.Count ();
         query = query.ApplyOrdering (queryObj, columnMapper);
         query = query.ApplyPaging (queryObj);
         return (count, query);
      }

      public static Paging<T> FetchPaging<T> (this IQueryable<T> query, IQueryObject queryObj, QueryColumnMapper<T> columnMapper = null) {
         columnMapper = columnMapper.FixColumnMapper();
         var res = query.ApplyEverythingWithCount (queryObj, columnMapper);
         return new Paging<T> () { Items = res.DataQuery.ToList (), TotalItems = res.Count };
      }

  #endregion    
#region "EntityFramework"
      // EntityFramework Integration (need EntityFramework  as a Dependency)
      // public async static Task < (int Count, IQueryable<T> DataQuery) > ApplyQueryWithCountAsync<T> (this IQueryable<T> query, IQueryObject queryObj, QueryColumnMapper<T> columnMapper) {
      //    query = query.ApplyFiltering (queryObj, columnMapper);
      //    var count = await query.CountAsync ();
      //    query = query.ApplyOrdering (queryObj, columnMapper);
      //    query = query.ApplyPaging (queryObj);
      //    return (count, query);
      // }
      // public async static Task<Paging<T>> FetchAsync<T> (this IQueryable<T> query, IQueryObject queryObj, QueryColumnMapper<T> columnMapper) {
      //    var res = await query.ApplyQueryWithCountAsync (queryObj, columnMapper);
      //    return new Paging<T> () { Items = await res.DataQuery.ToListAsync (), TotalItems = res.Count };
      // }
      // public async static Task<Paging<T>> FetchAsync<T> (this IQueryable<T> query, IQueryObject queryObj) 
      //    => await query.FetchAsync<T> (queryObj, GetDefaultColumnMapper<T> ());
  #endregion
#region "AutoMapper"
      // AutoMapper Integration (need AutoMapper as a Dependency)
      // public async static Task<Paging<TDestination>> FetchToAsync<TSource, TDestination> (this IQueryable<TSource> query, IQueryObject queryObj, QueryColumnMapper<TSource> columnMapper) {
      //    var res = await query.ApplyQueryWithCountAsync (queryObj, columnMapper);
      //    return new Paging<TDestination> () { Items = await res.DataQuery.ProjectTo<TDestination> ().ToListAsync (), TotalItems = res.Count };
      // }
      // public async static Task<Paging<D>> FetchToAsync<T, D> (this IQueryable<T> query, IQueryObject queryObj) 
      //    => await FetchToAsync<T, D> (query, queryObj, GetDefaultColumnMapper<T> ());
   }
#endregion
}