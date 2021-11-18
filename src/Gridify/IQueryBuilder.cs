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
      IQueryBuilder<T> AddMapper(IGridifyMapper<T> mapper);
      IQueryBuilder<T> AddCondition(string condition);
      IQueryBuilder<T> AddCondition(IGridifyFiltering condition);
      IQueryBuilder<T> AddQuery(IGridifyQuery gridifyQuery);
      IQueryBuilder<T> AddOrderBy(string orderBy);
      IQueryBuilder<T> AddPaging(int page, int pageSize);
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
      IQueryable<T> Build(IQueryable<T> context);
      IEnumerable<T> Build(IEnumerable<T> collection);
      Paging<T> BuildWithPaging(IEnumerable<T> collection);
      Paging<T> BuildWithPaging(IQueryable<T> collection);
      QueryablePaging<T> BuildWithQueryablePaging(IQueryable<T> collection);
   }
}
