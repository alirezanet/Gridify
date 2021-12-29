using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
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

      var syntaxTree = SyntaxTree.Parse(gridifyFiltering.Filter!);

      if (syntaxTree.Diagnostics.Any())
         throw new GridifyFilteringException(syntaxTree.Diagnostics.Last()!);

      if (mapper is null)
         mapper = new GridifyMapper<T>(true);

      mapper = mapper.FixMapper(syntaxTree);
      var (queryExpression, _) = ExpressionToQueryConvertor.GenerateQuery(syntaxTree.Root, mapper);
      if (queryExpression == null) throw new GridifyQueryException("Can not create expression with current data");
      return queryExpression;
   }

   public static IEnumerable<(LambdaExpression, bool, bool)> GetOrderingExpressions<T>(this IGridifyOrdering gridifyOrdering,
      IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(gridifyOrdering.OrderBy))
         throw new GridifyQueryException("OrderBy is not defined or not Found");

      var isFirst = true;
      var orders = ParseOrderings(gridifyOrdering.OrderBy!);

      if (mapper is null)
      {
         foreach (var (member, isAscending) in orders)
         {
            LambdaExpression? exp = null;
            try
            {
               exp = GridifyMapper<T>.CreateLambdaExpression(member);
            }
            catch (Exception)
            {
               // skip if there is no mappings available
            }

            if (exp != null)
            {
               if (isFirst)
               {
                  isFirst = false;
                  yield return (exp, isAscending, true);
               }
               else
               {
                  yield return (exp, isAscending, false);
               }
            }
         }
      }
      else
      {
         foreach (var (member, isAscending) in orders.Where(m => mapper.HasMap(m.memberName)))
         {
            if (!mapper.HasMap(member))
            {
               // skip if there is no mappings available
               if (mapper.Configuration.IgnoreNotMappedFields)
                  continue;

               throw new GridifyMapperException($"Mapping '{member}' not found");
            }
            if (isFirst)
            {
               isFirst = false;
               yield return (mapper.GetLambdaExpression(member)!, isAscending, true);
            }
            else
            {
               yield return (mapper.GetLambdaExpression(member)!, isAscending, false);
            }
         }
      }
   }

   public static LambdaExpression GetSelectingExpression<T>(this IGridifySelecting gridifySelecting, IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(gridifySelecting.Select))
         throw new GridifyQueryException("Select is not defined");

      if (mapper is null)
         mapper = new GridifyMapper<T>(true);

      var members = ParseSelectings(gridifySelecting.Select!).ToList();

      List<Expression>? exp = new List<Expression>();
      List<DynamicProperty> ldps = new();

      var Tmembers = typeof(T).GetProperties();

      var paramexp = Expression.Parameter(typeof(T));

      if (mapper is null)
      {
         foreach (var member in members)
         {
            Expression<Func<T, object>>? mexp = null;
            try
            {
               mexp = GridifyMapper<T>.CreateExpression(member);
            }
            catch (Exception)
            {
               // skip if there is no mappings available
            }

            if (mexp != null)
            {
               ldps.Add(new DynamicProperty(member, mexp.Type));
               //exp.Add(expm);
            }

         }
      }
      else
      {
         foreach (var member in members.Where(mapper.HasMap))
         {
            var tm = mapper.Configuration.CaseSensitive ? Tmembers.FirstOrDefault(m => m.Name.Equals(member)) : Tmembers.FirstOrDefault(m => m.Name.Equals(member, StringComparison.InvariantCultureIgnoreCase));

            var mexp = Expression.MakeMemberAccess(paramexp, tm!);
            ldps.Add(new DynamicProperty(mexp!.Member.Name, mexp!.Type));
            exp.Add(mexp);
         }
      }

      var newexp = CreateNewExpression(ldps, exp, null);

      var lambda = Expression.Lambda(newexp, paramexp);
      return lambda;
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
      {
         try
         {
            mapper.AddMap(field.FieldToken.Text);
         }
         catch (Exception)
         {
            if (!mapper.Configuration.IgnoreNotMappedFields)
               throw new GridifyMapperException($"Property '{field.FieldToken.Text}' not found.");
         }
      }

      return mapper;
   }

   private static IEnumerable<SyntaxNode> Descendants(this SyntaxNode root)
   {
      var nodes = new Stack<SyntaxNode>(new[] { root });
      while (nodes.Any())
      {
         SyntaxNode node = nodes.Pop();
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
   /// <param name="startWithThenBy">if you already have an ordering with start with ThenBy, new orderings will add on top of your orders</param>
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
   /// <param name="startWithThenBy">if you already have an ordering with start with ThenBy, new orderings will add on top of your orders</param>
   /// <typeparam name="T">type of target entity</typeparam>
   /// <returns>returns user query after applying Ordering </returns>
   public static IQueryable<T> ApplyOrdering<T>(this IQueryable<T> query, string orderBy, IGridifyMapper<T>? mapper = null,
      bool startWithThenBy = false)
   {
      return string.IsNullOrWhiteSpace(orderBy)
         ? query
         : ProcessOrdering(query, orderBy, startWithThenBy, mapper);
   }

   public static IQueryable ApplySelect<T>(this IQueryable<T> query, IGridifySelecting? gridifySelecting, IGridifyMapper<T>? mapper = null)
   {
      if (gridifySelecting==null || string.IsNullOrWhiteSpace(gridifySelecting!.Select))
         return (IQueryable<object>)query;

      if (mapper is null)
         mapper = new GridifyMapper<T>(true);

      var lambda = gridifySelecting.GetSelectingExpression(mapper);

      var callTypes = new[] { query.ElementType, lambda.Body.Type };
      var argument_exps = new Expression[] { query.Expression, Expression.Quote(lambda) };

      var selectMethodInfo = typeof(Queryable).GetMethods().First(n => n.Name == nameof(Queryable.Select));

      var argLenght = selectMethodInfo.GetGenericArguments().Length;

      selectMethodInfo =
         argLenght == 2
         ? selectMethodInfo.MakeGenericMethod(query.ElementType, lambda.Body.Type)
         : selectMethodInfo.MakeGenericMethod(lambda.Body.Type);

      var resultExpression = Expression.Call(null, selectMethodInfo, argument_exps);
      return query.Provider.CreateQuery(resultExpression);
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
      return  ((IGridifyFiltering)gridifyQuery).IsValid(mapper) &&
              ((IGridifyOrdering)gridifyQuery).IsValid(mapper) &&
              ((IGridifySelecting)gridifyQuery).IsValid(mapper)
              ;
   }

   public static bool IsValid<T>(this IGridifyFiltering filtering, IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(filtering.Filter)) return true;
      try
      {
         var parser = new Parser(filtering.Filter!);
         var syntaxTree = parser.Parse();
         if (syntaxTree.Diagnostics.Any())
            return false;

         var fieldExpressions = syntaxTree.Root.Descendants()
            .Where(q=> q.Kind is SyntaxKind.FieldExpression)
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
         if (orders.Any(order => !mapper.HasMap(order.memberName)))
            return false;
      }
      catch (Exception)
      {
         return false;
      }

      return true;
   }

   public static bool IsValid<T>(this IGridifySelecting selecting, IGridifyMapper<T>? mapper = null)
   {
      if (string.IsNullOrWhiteSpace(selecting.Select)) return true;
      try
      {
         var selects = ParseSelectings(selecting.Select!).ToList();
         mapper ??= new GridifyMapper<T>(true);
         if (selects.Any(order => !mapper.HasMap(order)))
            return false;
      }
      catch (Exception)
      {
         return false;
      }

      return true;
   }

   private static IQueryable<T> ProcessOrdering<T>(IQueryable<T> query, string orderings, bool startWithThenBy, IGridifyMapper<T>? mapper)
   {
      var isFirst = !startWithThenBy;

      var orders = ParseOrderings(orderings).ToList();

      // build the mapper if it is null
      if (mapper is null)
      {
         mapper = new GridifyMapper<T>();
         foreach (var (member, _) in orders)
         {
            try
            {
               mapper.AddMap(member);
            }
            catch (Exception)
            {
               if (!mapper.Configuration.IgnoreNotMappedFields)
                  throw new GridifyMapperException($"Mapping '{member}' not found");
            }
         }
      }

      foreach (var (member, isAscending) in orders)
      {
         if (!mapper.HasMap(member))
         {
            // skip if there is no mappings available
            if (mapper.Configuration.IgnoreNotMappedFields)
               continue;

            throw new GridifyMapperException($"Mapping '{member}' not found");
         }

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
               "desc" => false,
               "asc" => true,
               _ => throw new GridifyOrderingException("Invalid keyword. expected 'desc' or 'asc'")
            };
            yield return (spliced.First(), isAsc);
         }
         else
            yield return (orderingExp, true);
      }
   }

   private static IEnumerable<string> ParseSelectings(string selects)
   {
      foreach (var field in selects.Split(','))
      {
         var selectingExp = field.Trim();

         var spliced = selectingExp.Split(new[] { ' ', '\t' }, StringSplitOptions.RemoveEmptyEntries);

         yield return spliced.First();
      }
   }

   //This original fork from https://github.com/zzzprojects/System.Linq.Dynamic.Core\src\System.Linq.Dynamic.Core\Parser\ExpressionParser.cs
   public static Expression CreateNewExpression(IEnumerable<DynamicProperty> properties, IEnumerable<Expression> expressions, Type newType, bool _createParameterCtor = false)
   {
      // http://solutionizing.net/category/linq/
      Type? type = newType;

      if (type == null)
      {
         type = DynamicClassFactory.CreateType(properties, _createParameterCtor);
      }

      IEnumerable<PropertyInfo> propertyInfos = type.GetProperties();
      if (type.GetTypeInfo().BaseType == typeof(DynamicClass))
      {
         propertyInfos = propertyInfos.Where(x => x.Name != "Item").ToList();
      }

      Type[] propertyTypes = propertyInfos.Select(p => p.PropertyType).ToArray();
      ConstructorInfo ctor = type.GetConstructor(propertyTypes)!;
      if (ctor != null && ctor.GetParameters().Length == expressions.Count())
      {
         var expressionsPromoted = new List<Expression>();

         // Loop all expressions and promote if needed
         for (int i = 0; i < propertyTypes.Length; i++)
         {
            Type propertyType = propertyTypes[i];

            // Promote from Type to Nullable Type if needed
            if (expressions.ElementAt(i).NodeType != ExpressionType.MemberAccess)
            {
               expressionsPromoted.Add(Expression.MakeMemberAccess(expressions.ElementAt(i), propertyType.GetTypeInfo())!);
            }
            else
            {
               expressionsPromoted.Add(expressions.ElementAt(i));
            }
         }
         var ntc = Expression.New(ctor, expressionsPromoted, propertyInfos);
         return ntc;
      }

      MemberBinding[] bindings = new MemberBinding[properties.Count()];
      for (int i = 0; i < bindings.Length; i++)
      {
         string propertyOrFieldName = properties.ElementAt(i).Name;
         Type propertyOrFieldType;
         MemberInfo memberInfo;
         PropertyInfo propertyInfo = type.GetProperty(propertyOrFieldName)!;
         if (propertyInfo != null)
         {
            memberInfo = propertyInfo;
            propertyOrFieldType = propertyInfo.PropertyType;
         }
         else
         {
            FieldInfo fieldInfo = type.GetField(propertyOrFieldName)!;
            if (fieldInfo == null)
            {
               throw new Exception(string.Format("No property or field '{0}' exists in type '{1}'", propertyOrFieldName, type.GetTypeInfo().Name));
            }

            memberInfo = fieldInfo;
            propertyOrFieldType = fieldInfo.FieldType;
         }

         // Promote from Type to Nullable Type if needed
         if (expressions.ElementAt(i).NodeType != ExpressionType.MemberAccess)
         {
            bindings[i] = Expression.Bind(memberInfo, Expression.MakeMemberAccess(expressions.ElementAt(i), propertyOrFieldType.GetTypeInfo())!);
         }
         else
         {
            bindings[i] = Expression.Bind(memberInfo, expressions.ElementAt(i));
         }
      }

      return Expression.MemberInit(Expression.New(type), bindings); ;
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

      var syntaxTree = SyntaxTree.Parse(filter!);

      if (syntaxTree.Diagnostics.Any())
         throw new GridifyFilteringException(syntaxTree.Diagnostics.Last()!);

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
   /// <returns>returns a <c>QueryablePaging<T><c /> after applying filtering, ordering and paging</returns>
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
