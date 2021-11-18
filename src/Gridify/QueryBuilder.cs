using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gridify.Syntax;

namespace Gridify
{
   public class QueryBuilder<T> : IQueryBuilder<T>
   {
      private readonly List<LambdaExpression> _conditions = new();
      private IGridifyMapper<T>? _mapper;
      private string _orderBy = string.Empty;
      private (int page, int pageSize)? _paging;

      public IQueryBuilder<T> AddMapper(IGridifyMapper<T> mapper)
      {
         _mapper = mapper;
         return this;
      }

      public IQueryBuilder<T> AddCondition(string condition)
      {
         _conditions.Add(ConvertConditionToExpression(condition));
         return this;
      }

      public IQueryBuilder<T> AddCondition(IGridifyFiltering condition)
      {
         if (condition.Filter != null)
            _conditions.Add(ConvertConditionToExpression(condition.Filter));
         return this;
      }

      public IQueryBuilder<T> AddQuery(IGridifyQuery gridifyQuery)
      {
         if (gridifyQuery.Filter != null)
            _conditions.Add(ConvertConditionToExpression(gridifyQuery.Filter));

         if (!string.IsNullOrEmpty(gridifyQuery.OrderBy))
            _orderBy = gridifyQuery.OrderBy!;

         if (gridifyQuery.PageSize == 0) gridifyQuery.PageSize = GridifyExtensions.DefaultPageSize;
         _paging = (gridifyQuery.Page, gridifyQuery.PageSize);

         return this;
      }

      public IQueryBuilder<T> AddOrderBy(string orderBy)
      {
         _orderBy = orderBy;
         return this;
      }

      public IQueryBuilder<T> AddPaging(int page, int pageSize)
      {
         _paging = (page, pageSize);
         return this;
      }

      public IQueryBuilder<T> ConfigureDefaultMapper(GridifyMapperConfiguration mapperConfiguration)
      {
         _mapper = new GridifyMapper<T>(mapperConfiguration, true);
         return this;
      }

      public IQueryBuilder<T> ConfigureDefaultMapper(Action<GridifyMapperConfiguration> mapperConfiguration)
      {
         var mapperConfigurationInstance = new GridifyMapperConfiguration();
         mapperConfiguration(mapperConfigurationInstance);
         return ConfigureDefaultMapper(mapperConfigurationInstance);
      }

      public IQueryBuilder<T> AddMap(IGMap<T> map, bool overwrite = true)
      {
         _mapper ??= new GridifyMapper<T>(true);
         _mapper.AddMap(map, overwrite);
         return this;
      }

      public IQueryBuilder<T> RemoveMap(IGMap<T> map)
      {
         _mapper ??= new GridifyMapper<T>(true);
         _mapper.RemoveMap(map);
         return this;
      }

      public Expression<Func<T, bool>> BuildFilteringExpression()
      {
         if (_conditions.Count == 0)
            return _ => true;

         return (_conditions.Aggregate(null, (LambdaExpression? x, LambdaExpression y)
            => x is null ? y : x.And(y)) as Expression<Func<T, bool>>)!;
      }

      public IEnumerable<Expression<Func<T, object>>> BuildOrderingExpression()
      {
         if (string.IsNullOrEmpty(_orderBy)) throw new GridifyOrderingException("Please use 'AddOrderBy' to specify at least an single order");

         var gm = new GridifyQuery() { OrderBy = _orderBy };
         _mapper ??= new GridifyMapper<T>(true);
         return gm.GetOrderingExpressions(_mapper);
      }

      public Func<IQueryable<T>, bool> BuildQueryableEvaluator()
      {
         return collection =>
         {
            return _conditions.Count == 0 ||
                   _conditions.Aggregate(true, (current, expression) =>
                      current & collection.Any((expression as Expression<Func<T, bool>>)!));
         };
      }

      public Func<IEnumerable<T>, bool> BuildCollectionEvaluator()
      {
         return collection =>
         {
            return _conditions.Count == 0 ||
                   _conditions.Aggregate(true, (current, expression)
                      => current & collection.Any((expression as Expression<Func<T, bool>>)!.Compile()));
         };
      }

      public bool Evaluate(IQueryable<T> query)
      {
         return BuildQueryableEvaluator()(query);
      }

      public bool Evaluate(IEnumerable<T> collection)
      {
         return BuildCollectionEvaluator()(collection);
      }

      public IQueryable<T> Build(IQueryable<T> context)
      {
         var query = context;

         if (_conditions.Count > 0)
            query = query.Where(BuildFilteringExpression());

         if (!string.IsNullOrEmpty(_orderBy))
            query = query.ApplyOrdering(_orderBy);

         if (_paging.HasValue)
            query = query.Skip(_paging.Value.page * _paging.Value.pageSize).Take(_paging.Value.pageSize);

         return query;
      }

      public IEnumerable<T> Build(IEnumerable<T> collection)
      {
         if (_conditions.Count > 0)
            collection = collection.Where(BuildFilteringExpression().Compile());

         if (!string.IsNullOrEmpty(_orderBy))
            collection = collection.AsQueryable().ApplyOrdering(_orderBy);

         if (_paging.HasValue)
            collection = collection.Skip(_paging.Value.page * _paging.Value.pageSize).Take(_paging.Value.pageSize);

         return collection;
      }

      public Paging<T> BuildWithPaging(IEnumerable<T> collection)
      {
         var query = collection.AsQueryable();
         return BuildWithPaging(query);
      }

      public Paging<T> BuildWithPaging(IQueryable<T> collection)
      {
         var (count, query) = BuildWithQueryablePaging(collection);
         return new Paging<T>(count, query);
      }

      public QueryablePaging<T> BuildWithQueryablePaging(IQueryable<T> collection)
      {
         var query = collection;
         if (_conditions.Count > 0)
            query = query.Where(BuildFilteringExpression());

         if (!string.IsNullOrEmpty(_orderBy))
            query = query.ApplyOrdering(_orderBy);

         var count = query.Count();

         if (_paging.HasValue)
            query = query.Skip(_paging.Value.page * _paging.Value.pageSize).Take(_paging.Value.pageSize);

         return new QueryablePaging<T>(count, query);
      }

      private Expression<Func<T, bool>> ConvertConditionToExpression(string condition)
      {
         var syntaxTree = SyntaxTree.Parse(condition);

         if (syntaxTree.Diagnostics.Any())
            throw new GridifyFilteringException(syntaxTree.Diagnostics.Last()!);

         _mapper = _mapper.FixMapper();
         var (queryExpression, _) = ExpressionToQueryConvertor.GenerateQuery(syntaxTree.Root, _mapper);
         if (queryExpression == null) throw new GridifyQueryException($"Filter condition is not valid, '{condition}'.");
         return queryExpression;
      }
   }
}
