using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gridify
{
   public interface IQueryBuilder<T>
   {
      /// <summary>
      /// Using this method you can add a custom gridify mapper that will be used to map
      /// your provided string condition to a lambda expression.
      /// also when you use this method for the second time, it will replace the previous one.
      /// </summary>
      /// <param name="mapper"></param>
      /// <returns>returns IQueryBuilder</returns>
      IQueryBuilder<T> UseCustomMapper(IGridifyMapper<T> mapper);

      /// <summary>
      /// Using this method the default gridify mapper has no predefined mappings and
      /// you need to manually add your maps to the mapper using AddMap method.
      /// mapper will be used to convert your provided string conditions to a lambda expression.
      /// also when you use this method, previous mapper will be replaced,
      /// so make sure to use this before AddMap method.
      /// </summary>
      /// <param name="mapperConfiguration">optional mapper configuration</param>
      /// <returns>returns IQueryBuilder</returns>
      IQueryBuilder<T> UseEmptyMapper(GridifyMapperConfiguration mapperConfiguration);

      /// <inheritdoc cref="UseEmptyMapper(Gridify.GridifyMapperConfiguration)" />
      IQueryBuilder<T> UseEmptyMapper(Action<GridifyMapperConfiguration> mapperConfiguration);

      /// <summary>
      /// Using this method you can add gridify supported string base filtering statements
      /// </summary>
      /// <example> (Name=John,Age>10) </example>
      /// <param name="condition">string based filtering</param>
      /// <returns>returns IQueryBuilder</returns>
      IQueryBuilder<T> AddCondition(string condition);

      /// <summary>
      /// Using this method you can use GridifyQuery to add only filtering part
      /// </summary>
      /// <param name="condition">Accepts IGridifyFiltering so we can pass GridifyQuery object</param>
      /// <returns>returns IQueryBuilder</returns>
      IQueryBuilder<T> AddCondition(IGridifyFiltering condition);

      /// <summary>
      /// Using this method you can use GridifyQuery object to add filtering and sorting and configure paging
      /// </summary>
      /// <param name="gridifyQuery">Accept IGridifyQuery so we can pass GridifyQuery object</param>
      /// <returns>returns IQueryBuilder</returns>
      IQueryBuilder<T> AddQuery(IGridifyQuery gridifyQuery);

      IQueryBuilder<T> AddOrderBy(string orderBy);
      IQueryBuilder<T> ConfigurePaging(int page, int pageSize);
      IQueryBuilder<T> ConfigureDefaultMapper(GridifyMapperConfiguration mapperConfiguration);
      IQueryBuilder<T> ConfigureDefaultMapper(Action<GridifyMapperConfiguration> mapperConfiguration);
      IQueryBuilder<T> AddMap(IGMap<T> map, bool overwrite = true);
      IQueryBuilder<T> AddMap(string from, Expression<Func<T, object?>> to, Func<string, object>? convertor = null, bool overwrite = true);
      IQueryBuilder<T> RemoveMap(IGMap<T> map);
      Expression<Func<T, bool>> BuildFilteringExpression();
      IEnumerable<Expression<Func<T, object>>> BuildOrderingExpression();
      Func<IQueryable<T>, bool> BuildQueryableEvaluator();
      Func<IEnumerable<T>, bool> BuildCollectionEvaluator();
      bool Evaluate(IQueryable<T> query);
      bool Evaluate(IEnumerable<T> collection);

      /// <summary>
      /// Returns a delegate that can be used to apply the filtering, ordering and paging to a queryable.
      ///  </summary>
      /// <example>
      /// <code>
      /// var func = builder.Build();
      /// var query = func(queryableContext);
      /// </code>
      /// </example>
      /// <returns>A delegate as type <![CDATA[Func<IQueryable<T>, IQueryable<T>>]]></returns>
      Func<IQueryable<T>, IQueryable<T>> Build();

      /// <summary>
      /// Returns a delegate that can be used to apply the filtering, ordering and paging to a collection.
      ///  </summary>
      /// <example>
      /// <code>
      /// var func = builder.BuildAsEnumerable();
      /// var result = func(enumerableCollection);
      /// </code>
      /// </example>
      /// <returns>A delegate as type <![CDATA[Func<IEnumerable<T>, IEnumerable<T>>]]></returns>
      Func<IEnumerable<T>, IEnumerable<T>> BuildAsEnumerable();

      /// <summary>
      /// Directly applies the filtering, ordering and paging to a queryable.
      ///  </summary>
      /// <example>
      /// <code>
      /// var query = builder.Build(queryableContext);
      /// </code>
      /// </example>
      /// <returns><![CDATA[IQueryable<T>]]></returns>
      IQueryable<T> Build(IQueryable<T> context);

      /// <summary>
      /// Directly applies the filtering, ordering and paging to a enumerable collection.
      ///  </summary>
      /// <example>
      /// <code>
      /// var result = builder.Build(enumerableCollection);
      /// </code>
      /// </example>
      /// <returns><![CDATA[IEnumerable<T>]]></returns>
      IEnumerable<T> Build(IEnumerable<T> collection);

      /// <summary>
      /// Directly applies the filtering, ordering and paging to a enumerable collection.
      /// also returns the total count of the collection.
      ///  </summary>
      /// <example>
      /// <code>
      /// var pagingResult = builder.BuildWithPaging(enumerableCollection);
      /// // or
      /// var (count, result) = builder.BuildWithPaging(enumerableCollection);
      /// </code>
      /// </example>
      /// <returns><![CDATA[Paging<T>]]></returns>
      Paging<T> BuildWithPaging(IEnumerable<T> collection);

      /// <summary>
      /// Directly applies the filtering, ordering and paging to a queryable.
      /// also load the data and returns the total count of the records.
      ///  </summary>
      /// <example>
      /// <code>
      /// var pagingResult = builder.BuildWithPaging(queryableContext);
      /// // or
      /// var (count, result) = builder.BuildWithPaging(queryableContext);
      /// </code>
      /// </example>
      /// <returns><![CDATA[Paging<T>]]></returns>
      Paging<T> BuildWithPaging(IQueryable<T> collection);

      /// <summary>
      /// Directly applies the filtering, ordering and paging to a queryable.
      /// also returns the total count of the records.
      ///  </summary>
      /// <example>
      /// <code>
      /// var queryablePaging = builder.BuildWithQueryablePaging(queryableContext);
      /// // or
      /// var (count, query) = builder.BuildWithQueryablePaging(queryableContext);
      /// </code>
      /// </example>
      /// <returns><![CDATA[QueryablePaging<T>]]></returns>
      QueryablePaging<T> BuildWithQueryablePaging(IQueryable<T> collection);

      /// <summary>
      /// Returns a delegate that can be used to apply the filtering, ordering and paging to a queryable.
      /// also returns the total count of the records.
      /// </summary>
      /// <example>
      /// <code>
      /// var func = builder.BuildWithQueryablePaging();
      /// var query = func(queryableContext);
      /// </code>
      /// </example>
      /// <returns>A delegate as type <![CDATA[Func<IQueryable<T>,QueryablePaging<T>>]]></returns>
      Func<IQueryable<T>,QueryablePaging<T>> BuildWithQueryablePaging();

      /// <summary>
      /// Returns a delegate that can be used to apply the filtering, ordering and paging to a queryable.
      /// also load the data and returns the total count of the records.
      ///  </summary>
      /// <example>
      /// <code>
      /// var func = builder.BuildWithPaging();
      /// var pagingQuery = func(queryableContext);
      /// // or
      /// var (count, query) = func(queryableContext);
      /// </code>
      /// </example>
      /// <returns><![CDATA[ Func<IQueryable<T>,Paging<T>> ]]></returns>
      Func<IQueryable<T>,Paging<T>> BuildWithPaging();

      /// <summary>
      /// Returns a delegate that can be used to apply the filtering, ordering and paging to a enumerable collection.
      /// also load the data and returns the total count of the records.
      ///  </summary>
      /// <example>
      /// <code>
      /// var func = builder.BuildWithPagingAsEnumerable();
      /// var pagingResult = func(enumerableCollection);
      /// // or
      /// var (count, result) = func(enumerableCollection);
      /// </code>
      /// </example>
      /// <returns><![CDATA[ Func<IQueryable<T>,Paging<T>> ]]></returns>
      Func<IEnumerable<T>,Paging<T>> BuildWithPagingAsEnumerable();
   }
}
