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

      /// <inheritdoc />
      public IQueryBuilder<T> UseCustomMapper(IGridifyMapper<T> mapper)
      {
         _mapper = mapper;
         return this;
      }

      /// <inheritdoc />
      public IQueryBuilder<T> UseEmptyMapper(GridifyMapperConfiguration? mapperConfiguration = null)
      {
         _mapper = mapperConfiguration != null ? new GridifyMapper<T>(mapperConfiguration) : new GridifyMapper<T>();
         return this;
      }

      /// <inheritdoc />
      public IQueryBuilder<T> UseEmptyMapper(Action<GridifyMapperConfiguration> mapperConfiguration)
      {
         var mapperConfigurationInstance = new GridifyMapperConfiguration();
         mapperConfiguration(mapperConfigurationInstance);
         return UseEmptyMapper(mapperConfigurationInstance);
      }

      /// <inheritdoc />
      public IQueryBuilder<T> AddCondition(string condition)
      {
         _conditions.Add(ConvertConditionToExpression(condition));
         return this;
      }

      /// <inheritdoc />
      public IQueryBuilder<T> AddCondition(IGridifyFiltering condition)
      {
         if (condition.Filter != null)
            _conditions.Add(ConvertConditionToExpression(condition.Filter));
         return this;
      }

      /// <inheritdoc />
      public IQueryBuilder<T> AddQuery(IGridifyQuery gridifyQuery)
      {
         if (gridifyQuery.Filter != null)
            AddCondition(gridifyQuery.Filter);

         if (!string.IsNullOrEmpty(gridifyQuery.OrderBy))
            AddOrderBy(gridifyQuery.OrderBy!);

         if (gridifyQuery.PageSize == 0) gridifyQuery.PageSize = GridifyExtensions.DefaultPageSize;
            ConfigurePaging(gridifyQuery.Page, gridifyQuery.PageSize);

         return this;
      }

      /// <inheritdoc />
      public IQueryBuilder<T> AddOrderBy(string orderBy)
      {
         _orderBy = string.IsNullOrEmpty(_orderBy) ? orderBy : $"{_orderBy}, {orderBy}";
         return this;
      }

      /// <inheritdoc />
      public IQueryBuilder<T> ConfigurePaging(int page, int pageSize)
      {
         _paging = (page, pageSize);
         return this;
      }

      /// <inheritdoc />
      public IQueryBuilder<T> ConfigureDefaultMapper(GridifyMapperConfiguration mapperConfiguration)
      {
         if (_mapper != null && _mapper.GetCurrentMaps().Any())
         {
            var tempMapper = new GridifyMapper<T>(mapperConfiguration, true);
            _mapper.GetCurrentMaps().ToList().ForEach(map => tempMapper.AddMap(map));
            _mapper = tempMapper;
            return this;
         }

         _mapper = new GridifyMapper<T>(mapperConfiguration, true);
         return this;
      }

      /// <inheritdoc />
      public IQueryBuilder<T> ConfigureDefaultMapper(Action<GridifyMapperConfiguration> mapperConfiguration)
      {
         var mapperConfigurationInstance = new GridifyMapperConfiguration();
         mapperConfiguration(mapperConfigurationInstance);
         return ConfigureDefaultMapper(mapperConfigurationInstance);
      }

      /// <inheritdoc />
      public IQueryBuilder<T> AddMap(IGMap<T> map, bool overwrite = true)
      {
         _mapper ??= new GridifyMapper<T>(true);
         _mapper.AddMap(map, overwrite);
         return this;
      }

      /// <inheritdoc />
      public IQueryBuilder<T> AddMap(string from, Expression<Func<T, object?>> to, Func<string, object>? convertor = null, bool overwrite = true)
      {
         _mapper ??= new GridifyMapper<T>(true);
         _mapper.AddMap(from, to, convertor, overwrite);
         return this;
      }

      /// <inheritdoc />
      public IQueryBuilder<T> RemoveMap(IGMap<T> map)
      {
         _mapper ??= new GridifyMapper<T>(true);
         _mapper.RemoveMap(map);
         return this;
      }

      /// <inheritdoc />
      public Expression<Func<T, bool>> BuildFilteringExpression()
      {
         if (_conditions.Count == 0)
            return _ => true;

         return (_conditions.Aggregate(null, (LambdaExpression? x, LambdaExpression y)
            => x is null ? y : x.And(y)) as Expression<Func<T, bool>>)!;
      }

      /// <inheritdoc />
      public IEnumerable<Expression<Func<T, object>>> BuildOrderingExpression()
      {
         if (string.IsNullOrEmpty(_orderBy)) throw new GridifyOrderingException("Please use 'AddOrderBy' to specify at least an single order");

         var gm = new GridifyQuery { OrderBy = _orderBy };
         _mapper ??= new GridifyMapper<T>(true);
         return gm.GetOrderingExpressions(_mapper);
      }

      /// <inheritdoc />
      public Func<IQueryable<T>, bool> BuildQueryableEvaluator()
      {
         return collection =>
         {
            return _conditions.Count == 0 ||
                   _conditions.Aggregate(true, (current, expression) =>
                      current & collection.Any((expression as Expression<Func<T, bool>>)!));
         };
      }

      /// <inheritdoc />
      public Func<IEnumerable<T>, bool> BuildCollectionEvaluator()
      {
         return collection =>
         {
            return _conditions.Count == 0 ||
                   _conditions.Aggregate(true, (current, expression)
                      => current & collection.Any((expression as Expression<Func<T, bool>>)!.Compile()));
         };
      }

      /// <inheritdoc />
      public bool Evaluate(IQueryable<T> query)
      {
         return BuildQueryableEvaluator()(query);
      }

      /// <inheritdoc />
      public bool Evaluate(IEnumerable<T> collection)
      {
         return BuildCollectionEvaluator()(collection);
      }

      /// <inheritdoc />
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

      /// <inheritdoc />
      public Func<IQueryable<T>,IQueryable<T>> Build()
      {
         return Build;
      }

      /// <inheritdoc />
      public Func<IEnumerable<T>,IEnumerable<T>> BuildAsEnumerable()
      {
         return Build;
      }

      /// <inheritdoc />
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

      /// <inheritdoc />
      public Paging<T> BuildWithPaging(IEnumerable<T> collection)
      {
         var query = collection.AsQueryable();
         return BuildWithPaging(query);
      }

      /// <inheritdoc />
      public Paging<T> BuildWithPaging(IQueryable<T> collection)
      {
         var (count, query) = BuildWithQueryablePaging(collection);
         return new Paging<T>(count, query);
      }

      /// <inheritdoc />
      public Func<IQueryable<T>, QueryablePaging<T>> BuildWithQueryablePaging()
      {
         return BuildWithQueryablePaging;
      }

      /// <inheritdoc />
      public Func<IQueryable<T>, Paging<T>> BuildWithPaging()
      {
         return BuildWithPaging;
      }

      public Func<IEnumerable<T>, Paging<T>> BuildWithPagingAsEnumerable()
      {
         return BuildWithPaging;
      }


      /// <inheritdoc />
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

         return syntaxTree.CreateQuery(_mapper);
      }
   }
}
