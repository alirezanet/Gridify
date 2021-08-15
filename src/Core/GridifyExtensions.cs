using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gridify.Syntax;

namespace Gridify
{
   public static partial class GridifyExtensions
   {
      #region "Static Shared"

      public static int DefaultPageSize = 20;

      #endregion

      #region "Private"

      /// <summary>
      /// Set default <c>Page<c /> number and <c>PageSize<c /> if its not already set in gridifyQuery
      /// </summary>
      /// <param name="gridifyPagination">query and paging configuration</param>
      /// <returns>returns a IGridifyPagination with valid PageSize and Page</returns>
      private static IGridifyPagination FixPagingData(this IGridifyPagination gridifyPagination)
      {
         // set default for page number
         if (gridifyPagination.Page <= 0)
            gridifyPagination.Page = 1;

         // set default for PageSize
         if (gridifyPagination.PageSize <= 0)
            gridifyPagination.PageSize = DefaultPageSize;

         return gridifyPagination;
      }

      #endregion

      // TODO: should have some tests
      public static Expression<Func<T, bool>> GetFilteringExpression<T>(this IGridifyFiltering gridifyFiltering, IGridifyMapper<T>? mapper = null)
      {
         if (string.IsNullOrWhiteSpace(gridifyFiltering.Filter))
            throw new GridifyQueryException("Filter is not defined");

         mapper = mapper.FixMapper();

         var syntaxTree = SyntaxTree.Parse(gridifyFiltering.Filter!);

         if (syntaxTree.Diagnostics.Any())
            throw new GridifyFilteringException(syntaxTree.Diagnostics.Last()!);

         var queryExpression = ExpressionToQueryConvertor.GenerateQuery(syntaxTree.Root, mapper);
         if (queryExpression == null) throw new GridifyQueryException("Can not create expression with current data");
         return queryExpression;
      }

      public static IEnumerable<Expression<Func<T, object>>> GetOrderingExpressions<T>(this IGridifyOrdering gridifyOrdering, IGridifyMapper<T>? mapper = null)
      {
         mapper = mapper.FixMapper();

         if (string.IsNullOrWhiteSpace(gridifyOrdering.OrderBy))
            throw new GridifyQueryException("OrderBy is not defined or not Found");

         foreach (var (member, _) in ParseOrderings(gridifyOrdering.OrderBy!))
         {
            // skip if there is no mappings available
            if (!mapper.HasMap(member)) continue;
            
            yield return mapper.GetExpression(member)!;
         }
      }

      #region "Public"

      /// <summary>
      /// create and return a default <c>GridifyMapper</c>
      /// </summary>
      /// <typeparam name="T">type to set mappings</typeparam>
      /// <returns>returns an auto generated <c>GridifyMapper</c></returns>
      private static IGridifyMapper<T> GetDefaultMapper<T>()
      {
         return new GridifyMapper<T>().GenerateMappings();
      }

      /// <summary>
      /// if given mapper was null this function creates default generated mapper
      /// </summary>
      /// <param name="mapper">a <c>GridifyMapper<c /> that can be null</param>
      /// <typeparam name="T">type to set mappings</typeparam>
      /// <returns>return back mapper or new generated mapper if it was null</returns>
      public static IGridifyMapper<T> FixMapper<T>(this IGridifyMapper<T>? mapper)
      {
         return mapper ?? GetDefaultMapper<T>();
      }

      /// <summary>
      /// adds Filtering,Ordering And Paging to the query
      /// </summary>
      /// <param name="query">the original(target) queryable object</param>
      /// <param name="gridifyQuery">the configuration to apply paging, filtering and ordering</param>
      /// <param name="mapper">this is an optional parameter to apply filtering and ordering using a custom mapping configuration</param>
      /// <typeparam name="T">type of target entity</typeparam>
      /// <returns>returns user query after applying filtering, ordering and paging </returns>
      public static IQueryable<T> ApplyEverything<T>(this IQueryable<T> query, IGridifyQuery? gridifyQuery, IGridifyMapper<T>? mapper = null)
      {
         if (gridifyQuery == null) return query;
         mapper = mapper.FixMapper();

         query = query.ApplyFiltering(gridifyQuery, mapper);
         query = query.ApplyOrdering(gridifyQuery, mapper);
         query = query.ApplyPaging(gridifyQuery);
         return query;
      }

      /// <summary>
      /// adds Ordering to the query
      /// </summary>
      /// <param name="query">the original(target) queryable object</param>
      /// <param name="gridifyOrdering">the configuration to apply ordering</param>
      /// <param name="mapper">this is an optional parameter to apply ordering using a custom mapping configuration</param>
      /// <param name="startWithThenBy">if you already have an ordering with start with ThenBy, new orderings will add on top of your orders</param>
      /// <typeparam name="T">type of target entity</typeparam>
      /// <returns>returns user query after applying Ordering </returns>
      public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, IGridifyOrdering? gridifyOrdering, IGridifyMapper<T>? mapper = null, bool startWithThenBy = false)
      {
         if (gridifyOrdering == null) return query;
         mapper = mapper.FixMapper();
         return string.IsNullOrWhiteSpace(gridifyOrdering.OrderBy) ? query : ProcessOrdering(query, gridifyOrdering.OrderBy!,startWithThenBy, mapper);
      }

      private static IQueryable<T> ProcessOrdering<T>(IQueryable<T> query, string orderings, bool startWithThenBy, IGridifyMapper<T> mapper)
      {
         var isFirst = !startWithThenBy;
         foreach (var (member, isAscending) in ParseOrderings(orderings))
         {
            // skip if there is no mappings available
            if (!mapper.HasMap(member)) continue;
            
            if (isFirst)
            {
               query = query.OrderByMember(mapper.GetExpression(member), isAscending);
               isFirst = false;
            }
            else
               query = query.ThenByMember(mapper.GetExpression(member), isAscending);
         }
         return query;
      }

      private static IEnumerable<(string memberName, bool isAsc)> ParseOrderings(string orderings)
      {
         foreach (var field in orderings.Split(','))
         {
            var orderingExp = field.Trim();
            if (orderingExp.Contains(" "))
            {
               var spliced = orderingExp.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
               var isAsc = spliced.Last() switch
               {
                  "desc" =>  false,
                  "asc" => true,
                   _ => throw new GridifyOrderingException("Invalid keyword. expected 'desc' or 'asc'")
               };
               yield return (spliced.First(), isAsc);
            }
            else
               yield return (orderingExp, true);
         }
      }

      /// <summary>
      /// adds Ordering to the query
      /// </summary>
      /// <param name="query">the original(target) queryable object</param>
      /// <param name="gridifyOrdering">the configuration to apply ordering</param>
      /// <param name="groupOrder">select group member for ordering</param>
      /// // need to be more specific
      /// <param name="isGroupOrderAscending"></param>
      /// <param name="mapper">this is an optional parameter to apply ordering using a custom mapping configuration</param>
      /// <typeparam name="T">type of target entity</typeparam>
      /// <typeparam name="TKey">type of target property</typeparam>
      /// <returns>returns user query after applying Ordering </returns>
      public static IQueryable<T> ApplyOrdering<T, TKey>(this IQueryable<T> query, IGridifyOrdering? gridifyOrdering,
         Expression<Func<T, TKey>> groupOrder, bool isGroupOrderAscending = true,
         IGridifyMapper<T>? mapper = null)
      {
         if (gridifyOrdering == null) return query;
         mapper = mapper.FixMapper();
         if (string.IsNullOrWhiteSpace(gridifyOrdering.OrderBy))
            return query;

         query = isGroupOrderAscending
            ? query.OrderBy(groupOrder)
            : query.OrderByDescending(groupOrder);

         query = ProcessOrdering(query, gridifyOrdering.OrderBy!, true, mapper);
         return query;
      }

      public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, IGridifyPagination? gridifyPagination)
      {
         if (gridifyPagination == null) return query;
         gridifyPagination = gridifyPagination.FixPagingData();
         return query.Skip((gridifyPagination.Page - 1) * gridifyPagination.PageSize).Take(gridifyPagination.PageSize);
      }

      public static IQueryable<IGrouping<T2, T>> ApplyPaging<T, T2>(this IQueryable<IGrouping<T2, T>> query, IGridifyPagination? gridifyPagination)
      {
         if (gridifyPagination == null) return query;
         gridifyPagination = gridifyPagination.FixPagingData();
         return query.Skip((gridifyPagination.Page - 1) * gridifyPagination.PageSize).Take(gridifyPagination.PageSize);
      }

      public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, IGridifyFiltering? gridifyFiltering, IGridifyMapper<T>? mapper = null)
      {
         if (gridifyFiltering == null) return query;
         return string.IsNullOrWhiteSpace(gridifyFiltering.Filter) ? query : ApplyFiltering(query, gridifyFiltering.Filter, mapper);
      }

      public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, string? filter, IGridifyMapper<T>? mapper = null)
      {
         if (string.IsNullOrWhiteSpace(filter))
            return query;

         mapper = mapper.FixMapper();

         var syntaxTree = SyntaxTree.Parse(filter!);

         if (syntaxTree.Diagnostics.Any())
            throw new GridifyFilteringException(syntaxTree.Diagnostics.Last()!);

         var queryExpression = ExpressionToQueryConvertor.GenerateQuery(syntaxTree.Root, mapper);

         query = query.Where(queryExpression);

         return query;
      }

      public static IQueryable<T> ApplyFilterAndOrdering<T>(this IQueryable<T> query, IGridifyQuery? gridifyQuery, IGridifyMapper<T>? mapper = null)
      {
         mapper = mapper.FixMapper();
         query = query.ApplyFiltering(gridifyQuery, mapper);
         query = query.ApplyOrdering(gridifyQuery);
         return query;
      }

      public static IQueryable<T> ApplyOrderingAndPaging<T>(this IQueryable<T> query, IGridifyQuery? gridifyQuery, IGridifyMapper<T>? mapper = null)
      {
         mapper = mapper.FixMapper();
         query = query.ApplyOrdering(gridifyQuery, mapper);
         query = query.ApplyPaging(gridifyQuery);
         return query;
      }

      /// <summary>
      /// gets a query or collection,
      /// adds filtering,
      /// Get totalItems Count
      /// adds ordering and paging
      /// return QueryablePaging with TotalItems and an IQueryable Query
      /// </summary>
      /// <param name="query">the original(target) queryable object</param>
      /// <param name="gridifyQuery">the configuration to apply paging, filtering and ordering</param>
      /// <param name="mapper">this is an optional parameter to apply filtering and ordering using a custom mapping configuration</param>
      /// <typeparam name="T">type of target entity</typeparam>
      /// <returns>returns a <c>QueryablePaging<T><c /> after applying filtering, ordering and paging</returns>
      public static QueryablePaging<T> GridifyQueryable<T>(this IQueryable<T> query, IGridifyQuery? gridifyQuery, IGridifyMapper<T>? mapper = null)
      {
         mapper = mapper.FixMapper();
         query = query.ApplyFiltering(gridifyQuery, mapper);
         var count = query.Count();
         query = query.ApplyOrdering(gridifyQuery, mapper);
         query = query.ApplyPaging(gridifyQuery);
         return new QueryablePaging<T>(count, query);
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
      /// <returns>returns a loaded <c>Paging<T><c /> after applying filtering, ordering and paging </returns>
      /// <returns></returns>
      public static Paging<T> Gridify<T>(this IQueryable<T> query, IGridifyQuery? gridifyQuery, IGridifyMapper<T>? mapper = null)
      {
         mapper = mapper.FixMapper();
         var res = query.GridifyQueryable(gridifyQuery, mapper);
         return new Paging<T>(res.Count, res.Query.ToList());
      }

      /// <summary>
      /// gets a query or collection,
      /// adds filtering, ordering and paging
      /// loads filtered and sorted data
      /// return pagination ready result
      /// </summary>
      /// <param name="query">the original(target) queryable object</param>
      /// <param name="queryOption">the configuration to apply paging, filtering and ordering</param>
      /// <param name="mapper">this is an optional parameter to apply filtering and ordering using a custom mapping configuration</param>
      /// <typeparam name="T">type of target entity</typeparam>
      /// <returns>returns a loaded <c>Paging<T><c /> after applying filtering, ordering and paging </returns>
      /// <returns></returns>
      public static Paging<T> Gridify<T>(this IQueryable<T> query, Action<IGridifyQuery> queryOption, IGridifyMapper<T>? mapper = null)
      {
         mapper = mapper.FixMapper();
         var gridifyQuery = new GridifyQuery();
         queryOption?.Invoke(gridifyQuery);
         var res = query.GridifyQueryable(gridifyQuery, mapper);
         return new Paging<T>(res.Count, res.Query.ToList());
      }

      #endregion

      /// <summary>
      ///     Supports sorting of a given member as an expression when type is not known. It solves problem with LINQ to Entities unable to
      ///     cast different types as 'System.DateTime', 'System.DateTime?' to type 'System.Object'.
      ///     LINQ to Entities only supports casting Entity Data Model primitive types.
      /// </summary>
      /// <typeparam name="T">entity type</typeparam>
      /// <param name="query">query to apply sorting on.</param>
      /// <param name="expression">the member expression to apply</param>
      /// <param name="isSortAsc">the sort order to apply</param>
      /// <returns>Query with sorting applied as IOrderedQueryable of type T</returns>
      private static IOrderedQueryable<T> OrderByMember<T>(
         this IQueryable<T> query,
         Expression<Func<T, object>> expression,
         bool isSortAsc)
      {
         if (expression.Body is not UnaryExpression body) return isSortAsc ? query.OrderBy(expression) : query.OrderByDescending(expression);

         if (body.Operand is MemberExpression memberExpression)
         {
            return
               (IOrderedQueryable<T>)
               query.Provider.CreateQuery(
                  Expression.Call(
                     typeof(Queryable),
                     isSortAsc ? "OrderBy" : "OrderByDescending",
                     new[] { typeof(T), memberExpression.Type },
                     query.Expression,
                     Expression.Lambda(memberExpression, expression.Parameters)));
         }

         return isSortAsc ? query.OrderBy(expression) : query.OrderByDescending(expression);
      }

      /// <summary>
      ///     Supports sorting of a given member as an expression when type is not known. It solves problem with LINQ to Entities unable to
      ///     cast different types as 'System.DateTime', 'System.DateTime?' to type 'System.Object'.
      ///     LINQ to Entities only supports casting Entity Data Model primitive types.
      /// </summary>
      /// <typeparam name="T">entity type</typeparam>
      /// <param name="query">query to apply sorting on.</param>
      /// <param name="expression">the member expression to apply</param>
      /// <param name="isSortAsc">the sort order to apply</param>
      /// <returns>Query with sorting applied as IOrderedQueryable of type T</returns>
      public static IOrderedQueryable<T> ThenByMember<T>(
         this IQueryable<T> query,
         Expression<Func<T, object>> expression,
         bool isSortAsc)
      {
         return ((IOrderedQueryable<T>)query).ThenByMember(expression, isSortAsc);
      }

      /// <summary>
      ///     Supports sorting of a given member as an expression when type is not known. It solves problem with LINQ to Entities unable to
      ///     cast different types as 'System.DateTime', 'System.DateTime?' to type 'System.Object'.
      ///     LINQ to Entities only supports casting Entity Data Model primitive types.
      /// </summary>
      /// <typeparam name="T">entity type</typeparam>
      /// <param name="query">query to apply sorting on.</param>
      /// <param name="expression">the member expression to apply</param>
      /// <param name="isSortAsc">the sort order to apply</param>
      /// <returns>Query with sorting applied as IOrderedQueryable of type T</returns>
      private static IOrderedQueryable<T> ThenByMember<T>(
         this IOrderedQueryable<T> query,
         Expression<Func<T, object>> expression,
         bool isSortAsc)
      {
         if (expression.Body is not UnaryExpression body) return isSortAsc ? query.ThenBy(expression) : query.ThenByDescending(expression);
         if (body.Operand is MemberExpression memberExpression)
         {
            return
               (IOrderedQueryable<T>)
               query.Provider.CreateQuery(
                  Expression.Call(
                     typeof(Queryable),
                     isSortAsc ? "ThenBy" : "ThenByDescending",
                     new[] { typeof(T), memberExpression.Type },
                     query.Expression,
                     Expression.Lambda(memberExpression, expression.Parameters)));
         }

         return isSortAsc ? query.ThenBy(expression) : query.ThenByDescending(expression);
      }
   }
}