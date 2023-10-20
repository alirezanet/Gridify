using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.CompilerServices;
using Gridify.Syntax;

[assembly: InternalsVisibleTo("Gridify.EntityFramework")]

namespace Gridify;

public static partial class GridifyExtensions
{
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
         gridifyPagination.PageSize = GridifyGlobalConfiguration.DefaultPageSize;

      return gridifyPagination;
   }

   #endregion

   public static Expression<Func<T, bool>> GetFilteringExpression<T>(this IGridifyFiltering gridifyFiltering, IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(gridifyFiltering.Filter))
         throw new GridifyQueryException("Filter is not defined");

      var syntaxTree = SyntaxTree.Parse(gridifyFiltering.Filter!, GridifyGlobalConfiguration.CustomOperators.Operators);

      if (syntaxTree.Diagnostics.Any())
         throw new GridifyFilteringException(syntaxTree.Diagnostics.Last()!);

      mapper = mapper.FixMapper(syntaxTree);
      var (queryExpression, _) = ExpressionToQueryConvertor.GenerateQuery(syntaxTree.Root, mapper);
      if (queryExpression == null) throw new GridifyQueryException("Can not create expression with current data");
      return queryExpression;
   }

   public static IEnumerable<Expression<Func<T, object>>> GetOrderingExpressions<T>(this IGridifyOrdering gridifyOrdering,
      IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(gridifyOrdering.OrderBy))
         throw new GridifyQueryException("OrderBy is not defined or not Found");


      var members = ParseOrderings(gridifyOrdering.OrderBy!).Select(q => q.MemberName).ToList();
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
   private static IOrderedQueryable<T> OrderByMember<T>(
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
                  new[] { typeof(T), memberExpression.Type },
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
                  new[] { typeof(T), memberExpression.Type },
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
   /// <param name="mapper">a <c>GridifyMapper<c /> that can be null</param>
   /// <param name="syntaxTree">optional syntaxTree to Lazy mapping generation</param>
   /// <typeparam name="T">type to set mappings</typeparam>
   /// <returns>return back mapper or new generated mapper if it was null</returns>
   private static IGridifyMapper<T> FixMapper<T>(this IGridifyMapper<T>? mapper, SyntaxTree syntaxTree)
   {
      if (mapper != null) return mapper;

      mapper = new GridifyMapper<T>();
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

   /// <summary>
   /// adds Filtering,Ordering And Paging to the query
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
         : ProcessOrdering(query, gridifyOrdering.OrderBy!, startWithThenBy, mapper);
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
         : ProcessOrdering(query, orderBy, startWithThenBy, mapper);
   }

   public static IQueryable<object> ApplySelect<T>(this IQueryable<T> query, string props, IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(props))
         return (IQueryable<object>)query;

      if (mapper is null)
         mapper = new GridifyMapper<T>(true);

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

         var fieldExpressions = syntaxTree.Root.Descendants()
            .Where(q => q.Kind is SyntaxKind.FieldExpression)
            .Cast<FieldExpressionSyntax>().ToList();

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
         var orders = ParseOrderings(ordering.OrderBy!).ToList();
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

   internal static IQueryable<T> ProcessOrdering<T>(IQueryable<T> query, string orderings, bool startWithThenBy, IGridifyMapper<T>? mapper)
   {
      var isFirst = !startWithThenBy;

      var orders = ParseOrderings(orderings).ToList();

      // build the mapper if it is null
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
            query = query.OrderByMember(GetOrderExpression(order, mapper), order.IsAscending);
            isFirst = false;
         }
         else
         {
            query = query.ThenByMember(GetOrderExpression(order, mapper), order.IsAscending);
         }
      }

      return query;
   }

   internal static Expression<Func<T, object>> GetOrderExpression<T>(ParsedOrdering order, IGridifyMapper<T> mapper)
   {
      var exp = mapper.GetExpression(order.MemberName);
      switch (order.OrderingType)
      {
         case OrderingType.Normal:
            return exp;
         case OrderingType.NullCheck:
         case OrderingType.NotNullCheck:
         default:
         {
            // member should be nullable
            if (exp.Body is not UnaryExpression unary || Nullable.GetUnderlyingType(unary.Operand.Type) == null)
            {
               throw new GridifyOrderingException($"'{order.MemberName}' is not nullable type");
            }

            var prop = Expression.Property(exp.Parameters[0], order.MemberName);
            var hasValue = Expression.PropertyOrField(prop, "HasValue");

            switch (order.OrderingType)
            {
               case OrderingType.NullCheck:
               {
                  var boxedExpression = Expression.Convert(hasValue, typeof(object));
                  return Expression.Lambda<Func<T, object>>(boxedExpression, exp.Parameters);
               }
               case OrderingType.NotNullCheck:
               {
                  var notHasValue = Expression.Not(hasValue);
                  var boxedExpression = Expression.Convert(notHasValue, typeof(object));
                  return Expression.Lambda<Func<T, object>>(boxedExpression, exp.Parameters);
               }
               // should never reach here
               case OrderingType.Normal:
                  return exp;
               default:
                  throw new ArgumentOutOfRangeException();
            }
         }
      }
   }

   internal static string ReplaceAll(this string seed, IEnumerable<char> chars, char replacementCharacter)
   {
      return chars.Aggregate(seed, (str, cItem) => str.Replace(cItem, replacementCharacter));
   }

   private static IEnumerable<ParsedOrdering> ParseOrderings(string orderings)
   {
      var nullableChars = new[] { '?', '!' };
      foreach (var field in orderings.Split(','))
      {
         var orderingExp = field.Trim();
         if (orderingExp.Contains(" "))
         {
            var spliced = orderingExp.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);
            var isAsc = spliced.Last() switch
            {
               "desc" => false,
               "asc" => true,
               _ => throw new GridifyOrderingException("Invalid keyword. expected 'desc' or 'asc'")
            };
            var member = spliced.First();
            yield return new ParsedOrdering()
            {
               MemberName = member.ReplaceAll(nullableChars, ' ').TrimEnd(),
               IsAscending = isAsc,
               OrderingType = member.EndsWith("?") ? OrderingType.NullCheck
                    : member.EndsWith("!") ? OrderingType.NotNullCheck
                    : OrderingType.Normal
            };
         }
         else
         {
            yield return new ParsedOrdering()
            {
               MemberName = orderingExp.ReplaceAll(nullableChars, ' ').TrimEnd(),
               IsAscending = true,
               OrderingType = orderingExp.EndsWith("?") ? OrderingType.NullCheck
                  : orderingExp.EndsWith("!") ? OrderingType.NotNullCheck
                  : OrderingType.Normal
            };
         }
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

      if (string.IsNullOrWhiteSpace(gridifyOrdering.OrderBy))
         return query;

      query = isGroupOrderAscending
         ? query.OrderBy(groupOrder)
         : query.OrderByDescending(groupOrder);

      query = ProcessOrdering(query, gridifyOrdering.OrderBy!, true, mapper);
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
         throw new GridifyFilteringException(syntaxTree.Diagnostics[0]);

      mapper = mapper.FixMapper(syntaxTree);

      var (queryExpression, _) = ExpressionToQueryConvertor.GenerateQuery(syntaxTree.Root, mapper);

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
      var exp = ExpressionToQueryConvertor.GenerateQuery(syntaxTree.Root, mapper).Expression;
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
   /// <returns>returns a <c>QueryablePaging<T><c/> after applying filtering, ordering and paging</returns>
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
   /// <returns>returns a loaded <c>Paging<T><c /> after applying filtering, ordering and paging </returns>
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
   /// <returns>returns a loaded <c>Paging<T><c /> after applying filtering, ordering and paging </returns>
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
