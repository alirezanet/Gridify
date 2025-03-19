using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gridify.Builder;
using Gridify.Syntax;

namespace Gridify;

public static partial class GridifyExtensions
{
   /// <summary>
   /// Sets default <c>Page</c> number and <c>PageSize</c> if its not already set in gridifyQuery
   /// </summary>
   /// <param name="gridifyPagination">query and paging configuration</param>
   /// <returns>returns a IGridifyPagination with valid PageSize and Page</returns>
   internal static IGridifyPagination FixPagingData(this IGridifyPagination gridifyPagination)
   {
      // set default for page number
      if (gridifyPagination.Page <= 0)
         gridifyPagination.Page = 1;

      // set default for PageSize
      if (gridifyPagination.PageSize <= 0)
         gridifyPagination.PageSize = GridifyGlobalConfiguration.DefaultPageSize;

      return gridifyPagination;
   }

   public static Expression<Func<T, bool>> GetFilteringExpression<T>(this IGridifyFiltering gridifyFiltering, IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(gridifyFiltering.Filter))
         throw new GridifyQueryException("Filter is not defined");

      var syntaxTree = SyntaxTree.Parse(gridifyFiltering.Filter!, GridifyGlobalConfiguration.CustomOperators.Operators);

      if (syntaxTree.Diagnostics.Any())
         throw new GridifyFilteringException(syntaxTree.Diagnostics.Last()!);

      mapper = mapper.FixMapper(syntaxTree);
      var queryExpression = new LinqQueryBuilder<T>(mapper).Build(syntaxTree.Root);
      if (queryExpression == null) throw new GridifyQueryException("Can not create expression with current data");
      return queryExpression;
   }

   public static IEnumerable<Expression<Func<T, object>>> GetOrderingExpressions<T>(this IGridifyOrdering gridifyOrdering,
      IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(gridifyOrdering.OrderBy))
         throw new GridifyQueryException("OrderBy is not defined or not found");


      var members = gridifyOrdering.OrderBy!.ParseOrderings().Select(q => q.MemberName).ToList();
      if (mapper is null)
         foreach (var member in members)
         {
            Expression<Func<T, object>>? exp = null;
            try
            {
               exp = GridifyMapper<T>.CreateExpression(member);
            }
            catch (Exception)
            {
               // skip if there is no mappings available
            }

            if (exp != null) yield return exp;
         }
      else
         foreach (var member in members.Where(mapper.HasMap))
            yield return mapper.GetExpression(member)!;
   }

   /// <summary>
   /// Supports sorting of a given member as an expression when type is not known. It solves problem with LINQ to Entities
   /// unable to
   /// cast different types as 'System.DateTime', 'System.DateTime?' to type 'System.Object'.
   /// LINQ to Entities only supports casting Entity Data Model primitive types.
   /// </summary>
   /// <typeparam name="T">entity type</typeparam>
   /// <param name="query">query to apply sorting on.</param>
   /// <param name="expression">the member expression to apply</param>
   /// <param name="isSortAsc">the sort order to apply</param>
   /// <returns>Query with sorting applied as IOrderedQueryable of type T</returns>
   internal static IOrderedQueryable<T> OrderByMember<T>(
      this IQueryable<T> query,
      Expression<Func<T, object>> expression,
      bool isSortAsc)
   {
      if (expression.Body is not UnaryExpression body) return isSortAsc ? query.OrderBy(expression) : query.OrderByDescending(expression);

      if (body.Operand is MemberExpression memberExpression)
         return
            (IOrderedQueryable<T>)
            query.Provider.CreateQuery(
               Expression.Call(
                  typeof(Queryable),
                  isSortAsc ? "OrderBy" : "OrderByDescending",
                  [typeof(T), memberExpression.Type],
                  query.Expression,
                  Expression.Lambda(memberExpression, expression.Parameters)));

      return isSortAsc ? query.OrderBy(expression) : query.OrderByDescending(expression);
   }

   /// <summary>
   /// Supports sorting of a given member as an expression when type is not known. It solves problem with LINQ to Entities
   /// unable to
   /// cast different types as 'System.DateTime', 'System.DateTime?' to type 'System.Object'.
   /// LINQ to Entities only supports casting Entity Data Model primitive types.
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
   /// Supports sorting of a given member as an expression when type is not known. It solves problem with LINQ to Entities
   /// unable to
   /// cast different types as 'System.DateTime', 'System.DateTime?' to type 'System.Object'.
   /// LINQ to Entities only supports casting Entity Data Model primitive types.
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
         return
            (IOrderedQueryable<T>)
            query.Provider.CreateQuery(
               Expression.Call(
                  typeof(Queryable),
                  isSortAsc ? "ThenBy" : "ThenByDescending",
                  [typeof(T), memberExpression.Type],
                  query.Expression,
                  Expression.Lambda(memberExpression, expression.Parameters)));

      return isSortAsc ? query.ThenBy(expression) : query.ThenByDescending(expression);
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
   /// <param name="mapper">a <c>GridifyMapper</c> that can be null</param>
   /// <param name="syntaxTree">optional syntaxTree to Lazy mapping generation</param>
   /// <typeparam name="T">type to set mappings</typeparam>
   /// <returns>return back mapper or new generated mapper if it was null</returns>
   internal static IGridifyMapper<T> FixMapper<T>(this IGridifyMapper<T>? mapper, SyntaxTree syntaxTree)
   {
      if (mapper != null) return mapper;

      mapper = new GridifyMapper<T>();

      var fieldExpressions = syntaxTree.Root.DescendantsFieldExpressions();

      try
      {
         foreach (var fieldExpression in fieldExpressions) mapper.AddMap(fieldExpression.FieldToken.Text);
      }
      catch (Exception)
      {
         if (!mapper.Configuration.IgnoreNotMappedFields)
            throw;
      }

      return mapper;
   }

   /// <summary>
   /// Adds Filtering, Ordering And Paging to the query
   /// </summary>
   /// <param name="query">the original(target) queryable object</param>
   /// <param name="gridifyQuery">the configuration to apply paging, filtering and ordering</param>
   /// <param name="mapper">this is an optional parameter to apply filtering and ordering using a custom mapping configuration</param>
   /// <typeparam name="T">type of target entity</typeparam>
   /// <returns>returns user query after applying filtering, ordering and paging </returns>
   public static IQueryable<T> ApplyFilteringOrderingPaging<T>(this IQueryable<T> query, IGridifyQuery? gridifyQuery,
      IGridifyMapper<T>? mapper = null)
   {
      if (gridifyQuery == null) return query;

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
   /// <param name="startWithThenBy">
   /// if you already have an ordering with start with ThenBy, new orderings will add on top of
   /// your orders
   /// </param>
   /// <typeparam name="T">type of target entity</typeparam>
   /// <returns>returns user query after applying Ordering </returns>
   public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, IGridifyOrdering? gridifyOrdering, IGridifyMapper<T>? mapper = null,
      bool startWithThenBy = false)
   {
      if (gridifyOrdering == null) return query;
      return string.IsNullOrWhiteSpace(gridifyOrdering.OrderBy)
         ? query
         : new LinqSortingQueryBuilder<T>(mapper).ProcessOrdering(query, gridifyOrdering.OrderBy!, startWithThenBy);
   }

   /// <summary>
   /// adds Ordering to the query
   /// </summary>
   /// <param name="query">the original(target) queryable object</param>
   /// <param name="orderBy">the ordering fields</param>
   /// <param name="mapper">this is an optional parameter to apply ordering using a custom mapping configuration</param>
   /// <param name="startWithThenBy">
   /// if you already have an ordering with start with ThenBy, new orderings will add on top of
   /// your orders
   /// </param>
   /// <typeparam name="T">type of target entity</typeparam>
   /// <returns>returns user query after applying Ordering </returns>
   public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, string orderBy, IGridifyMapper<T>? mapper = null,
      bool startWithThenBy = false)
   {
      return string.IsNullOrWhiteSpace(orderBy)
         ? query
         : new LinqSortingQueryBuilder<T>(mapper).ProcessOrdering(query, orderBy, startWithThenBy);
   }

   public static IQueryable<object> ApplySelect<T>(this IQueryable<T> query, string props, IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(props))
         return (IQueryable<object>)query;

      mapper ??= new GridifyMapper<T>(true);

      var exp = mapper.GetExpression(props);
      var result = query.Select(exp);


      return result;
   }

   /// <summary>
   /// Validates Filter and/or OrderBy with Mappings
   /// </summary>
   /// <param name="gridifyQuery">gridify query with (Filter or OrderBy) </param>
   /// <param name="mapper">the gridify mapper that you want to use with, this is optional</param>
   /// <typeparam name="T"></typeparam>
   /// <returns></returns>
   public static bool IsValid<T>(this IGridifyQuery gridifyQuery, IGridifyMapper<T>? mapper = null)
   {
      return ((IGridifyFiltering)gridifyQuery).IsValid(mapper) &&
             ((IGridifyOrdering)gridifyQuery).IsValid(mapper);
   }

   public static bool IsValid<T>(this IGridifyFiltering filtering, IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(filtering.Filter)) return true;
      try
      {
         var parser = new Parser(filtering.Filter!, GridifyGlobalConfiguration.CustomOperators.Operators);
         var syntaxTree = parser.Parse();
         if (syntaxTree.Diagnostics.Any())
            return false;

         var fieldExpressions = syntaxTree.Root.DescendantsFieldExpressions();

         mapper ??= new GridifyMapper<T>(true);

         if (fieldExpressions.Any(field => !mapper.HasMap(field.FieldToken.Text)))
            return false;
      }
      catch (Exception)
      {
         return false;
      }

      return true;
   }

   public static bool IsValid<T>(this IGridifyOrdering ordering, IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(ordering.OrderBy)) return true;
      try
      {
         var orders = ordering.OrderBy!.ParseOrderings();
         mapper ??= new GridifyMapper<T>(true);
         if (orders.Any(order => !mapper.HasMap(order.MemberName)))
            return false;
      }
      catch (Exception)
      {
         return false;
      }

      return true;
   }

   internal static string ReplaceAll(this string seed, IEnumerable<char> chars, char replacementCharacter)
   {
      return chars.Aggregate(seed, (str, cItem) => str.Replace(cItem, replacementCharacter));
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

      if (string.IsNullOrWhiteSpace(gridifyOrdering.OrderBy))
         return query;

      query = isGroupOrderAscending
         ? query.OrderBy(groupOrder)
         : query.OrderByDescending(groupOrder);

      query = new LinqSortingQueryBuilder<T>(mapper).ProcessOrdering(query, gridifyOrdering.OrderBy!, true);
      return query;
   }

   public static IQueryable<T> ApplyPaging<T>(this IQueryable<T> query, int page, int pageSize)
   {
      return query.Skip((page - 1) * pageSize).Take(pageSize);
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

      var syntaxTree = SyntaxTree.Parse(filter!, GridifyGlobalConfiguration.CustomOperators.Operators);

      if (syntaxTree.Diagnostics.Any())
         throw new GridifyFilteringException(string.Join("\n", syntaxTree.Diagnostics.Reverse()));

      mapper = mapper.FixMapper(syntaxTree);

      var queryExpression = new LinqQueryBuilder<T>(mapper).Build(syntaxTree.Root);
      query = query.Where(queryExpression);

      return query;
   }

   public static IQueryable<T> ApplyFilteringAndOrdering<T>(this IQueryable<T> query, IGridifyQuery? gridifyQuery,
      IGridifyMapper<T>? mapper = null)
   {
      query = query.ApplyFiltering(gridifyQuery, mapper);
      query = query.ApplyOrdering(gridifyQuery, mapper);
      return query;
   }

   public static Expression<Func<T, bool>> CreateQuery<T>(this SyntaxTree syntaxTree, IGridifyMapper<T>? mapper = null)
   {
      mapper = mapper.FixMapper(syntaxTree);
      var exp = new LinqQueryBuilder<T>(mapper).Build(syntaxTree.Root);
      if (exp == null) throw new GridifyQueryException("Invalid SyntaxTree.");
      return exp;
   }

   public static IQueryable<T> ApplyOrderingAndPaging<T>(this IQueryable<T> query, IGridifyQuery? gridifyQuery, IGridifyMapper<T>? mapper = null)
   {
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
   /// <returns>returns a <c>QueryablePaging{T}</c> after applying filtering, ordering and paging</returns>
   public static QueryablePaging<T> GridifyQueryable<T>(this IQueryable<T> query, IGridifyQuery? gridifyQuery, IGridifyMapper<T>? mapper = null)
   {
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
   /// <returns>returns a loaded <c>Paging{T}</c> after applying filtering, ordering and paging </returns>
   /// <returns></returns>
   public static Paging<T> Gridify<T>(this IQueryable<T> query, IGridifyQuery? gridifyQuery, IGridifyMapper<T>? mapper = null)
   {
      var (count, queryable) = query.GridifyQueryable(gridifyQuery, mapper);
      return new Paging<T>(count, queryable.ToList());
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
   /// <returns>returns a loaded <c>Paging{T}</c> after applying filtering, ordering and paging </returns>
   /// <returns></returns>
   public static Paging<T> Gridify<T>(this IQueryable<T> query, Action<IGridifyQuery> queryOption, IGridifyMapper<T>? mapper = null)
   {
      var gridifyQuery = new GridifyQuery();
      queryOption.Invoke(gridifyQuery);
      var (count, queryable) = query.GridifyQueryable(gridifyQuery, mapper);
      return new Paging<T>(count, queryable.ToList());
   }

   #endregion
}
