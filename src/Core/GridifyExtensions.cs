using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gridify.Syntax;

namespace Gridify
{
   public static class GridifyExtensions
   {
      #region "Static Shared"

      public static int DefaultPageSize = 10;

      #endregion

      #region "Private"

      /// <summary>
      /// Set default <c>Page<c /> number and <c>PageSize<c /> if its not already set in gridifyQuery
      /// </summary>
      /// <param name="gridifyQuery">query and paging configuration</param>
      /// <returns>returns a gridifyQuery with valid PageSize and Page</returns>
      private static IGridifyQuery FixPagingData(this IGridifyQuery gridifyQuery)
      {
         // set default for page number
         if (gridifyQuery.Page <= 0)
            gridifyQuery.Page = 1;

         // set default for PageSize
         if (gridifyQuery.PageSize <= 0)
            gridifyQuery.PageSize = DefaultPageSize;

         return gridifyQuery;
      }

      #endregion

      // TODO: should have some tests
      public static Expression<Func<T, bool>> GetFilteringExpression<T>(this IGridifyQuery gridifyQuery, IGridifyMapper<T> mapper = null)
      {
         if (string.IsNullOrWhiteSpace(gridifyQuery.Filter))
            throw new GridifyQueryException("Filter is not defined");

         mapper = mapper.FixMapper();

         var syntaxTree = SyntaxTree.Parse(gridifyQuery.Filter);

         if (syntaxTree.Diagnostics.Any())
            throw new GridifyFilteringException(syntaxTree.Diagnostics.Last()!);

         var queryExpression = ExpressionToQueryConvertor.GenerateQuery(syntaxTree.Root, mapper);
         if (queryExpression == null) throw new GridifyQueryException("Can not create expression with current data");
         return queryExpression;
      }

      public static Expression<Func<T, object>> GetOrderingExpression<T>(this IGridifyQuery gridifyQuery, IGridifyMapper<T> mapper = null)
      {
         mapper = mapper.FixMapper();
         if (string.IsNullOrWhiteSpace(gridifyQuery.SortBy) || !mapper.HasMap(gridifyQuery.SortBy))
            throw new GridifyQueryException("SortBy is not defined or not Found");
         var expression = mapper.GetExpression(gridifyQuery.SortBy);
         return expression;
      }

      #region "Public"

      /// <summary>
      /// create and return a default <c>GridifyMapper</c>
      /// </summary>
      /// <typeparam name="T">type to set mappings</typeparam>
      /// <returns>returns an auto generated <c>GridifyMapper</c></returns>
      public static IGridifyMapper<T> GetDefaultMapper<T>()
      {
         return new GridifyMapper<T>().GenerateMappings();
      }

      /// <summary>
      /// if given mapper was null this function creates default generated mapper
      /// </summary>
      /// <param name="mapper">a <c>GridifyMapper<c /> that can be null</param>
      /// <typeparam name="T">type to set mappings</typeparam>
      /// <returns>return back mapper or new generated mapper if it was null</returns>
      public static IGridifyMapper<T> FixMapper<T>(this IGridifyMapper<T> mapper)
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
      public static IQueryable<T> ApplyEverything<T>(this IQueryable<T> query, IGridifyQuery gridifyQuery, IGridifyMapper<T> mapper = null)
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
      /// <param name="gridifyQuery">the configuration to apply ordering</param>
      /// <param name="mapper">this is an optional parameter to apply ordering using a custom mapping configuration</param>
      /// <typeparam name="T">type of target entity</typeparam>
      /// <returns>returns user query after applying Ordering </returns>
      public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, IGridifyQuery gridifyQuery, IGridifyMapper<T> mapper = null)
      {
         if (gridifyQuery == null) return query;
         mapper = mapper.FixMapper();
         if (string.IsNullOrWhiteSpace(gridifyQuery.SortBy) || !mapper.HasMap(gridifyQuery.SortBy))
            return query;

         return query.OrderByMember(mapper.GetExpression(gridifyQuery.SortBy), gridifyQuery.IsSortAsc);
      }

      /// <summary>
      /// adds Ordering to the query
      /// </summary>
      /// <param name="query">the original(target) queryable object</param>
      /// <param name="gridifyQuery">the configuration to apply ordering</param>
      /// <param name="groupOrder">select group member for ordering</param>
      /// // need to be more specific
      /// <param name="mapper">this is an optional parameter to apply ordering using a custom mapping configuration</param>
      /// <typeparam name="T">type of target entity</typeparam>
      /// <typeparam name="TKey">type of target property</typeparam>
      /// <returns>returns user query after applying Ordering </returns>
      public static IQueryable<T> ApplyOrdering<T, TKey>(this IQueryable<T> query, IGridifyQuery gridifyQuery, Expression<Func<T, TKey>> groupOrder,
         IGridifyMapper<T> mapper = null)
      {
         if (gridifyQuery == null) return query;
         mapper = mapper.FixMapper();
         if (string.IsNullOrWhiteSpace(gridifyQuery.SortBy) || !mapper.HasMap(gridifyQuery.SortBy))
            return query;

         return gridifyQuery.IsSortAsc
            ? query.OrderBy(groupOrder).ThenByMember(mapper.GetExpression(gridifyQuery.SortBy),gridifyQuery.IsSortAsc)
            : query.OrderByDescending(groupOrder).ThenByMember(mapper.GetExpression(gridifyQuery.SortBy),gridifyQuery.IsSortAsc);
      }

      public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, IGridifyQuery gridifyQuery)
      {
         if (gridifyQuery == null) return query;
         gridifyQuery = gridifyQuery.FixPagingData();
         return query.Skip((gridifyQuery.Page - 1) * gridifyQuery.PageSize).Take(gridifyQuery.PageSize);
      }

      public static IQueryable<IGrouping<T2, T>> ApplyPaging<T, T2>(this IQueryable<IGrouping<T2, T>> query, IGridifyQuery gridifyQuery)
      {
         if (gridifyQuery == null) return query;
         gridifyQuery = gridifyQuery.FixPagingData();
         return query.Skip((gridifyQuery.Page - 1) * gridifyQuery.PageSize).Take(gridifyQuery.PageSize);
      }

      public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, IGridifyQuery gridifyQuery, IGridifyMapper<T> mapper = null)
      {
         if (gridifyQuery == null) return query;
         return string.IsNullOrWhiteSpace(gridifyQuery.Filter) ? query : ApplyFiltering(query, gridifyQuery.Filter, mapper);
      }

      public static IQueryable<T> ApplyFiltering<T>(this IQueryable<T> query, string filter, IGridifyMapper<T> mapper = null)
      {
         if (string.IsNullOrWhiteSpace(filter))
            return query;

         mapper = mapper.FixMapper();

         var syntaxTree = SyntaxTree.Parse(filter);

         if (syntaxTree.Diagnostics.Any())
            throw new GridifyFilteringException(syntaxTree.Diagnostics.Last()!);

         var queryExpression = ExpressionToQueryConvertor.GenerateQuery(syntaxTree.Root, mapper);

         if (queryExpression != null)
            query = query.Where(queryExpression);

         return query;
      }

      public static IQueryable<T> ApplyFilterAndOrdering<T>(this IQueryable<T> query, IGridifyQuery gridifyQuery, IGridifyMapper<T> mapper = null)
      {
         mapper = mapper.FixMapper();
         query = query.ApplyFiltering(gridifyQuery, mapper);
         query = query.ApplyOrdering(gridifyQuery);
         return query;
      }


      public static IQueryable<T> ApplyOrderingAndPaging<T>(this IQueryable<T> query, IGridifyQuery gridifyQuery, IGridifyMapper<T> mapper = null)
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
      public static QueryablePaging<T> GridifyQueryable<T>(this IQueryable<T> query, IGridifyQuery gridifyQuery, IGridifyMapper<T> mapper = null)
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
      public static Paging<T> Gridify<T>(this IQueryable<T> query, IGridifyQuery gridifyQuery, IGridifyMapper<T> mapper = null)
      {
         mapper = mapper.FixMapper();
         var res = query.GridifyQueryable(gridifyQuery, mapper);
         return new Paging<T>(res.TotalItems, res.Query.ToList());
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
      public static Paging<T> Gridify<T>(this IQueryable<T> query, Action<IGridifyQuery> queryOption, IGridifyMapper<T> mapper = null)
      {
         mapper = mapper.FixMapper();
         var gridifyQuery = new GridifyQuery();
         queryOption?.Invoke(gridifyQuery);
         var res = query.GridifyQueryable(gridifyQuery, mapper);
         return new Paging<T>(res.TotalItems, res.Query.ToList());
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
                     new[] {typeof(T), memberExpression.Type},
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
         return ((IOrderedQueryable<T>) query).ThenByMember(expression, isSortAsc);
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
                     new[] {typeof(T), memberExpression.Type},
                     query.Expression,
                     Expression.Lambda(memberExpression, expression.Parameters)));
         }

         return isSortAsc ? query.ThenBy(expression) : query.ThenByDescending(expression);
      }
   }
}