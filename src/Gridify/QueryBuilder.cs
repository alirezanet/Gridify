using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Gridify.Syntax;

namespace Gridify;

public class QueryBuilder<T> : IQueryBuilder<T>
{
   private readonly List<string> _conditionList = new();
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
      _conditionList.Add(condition);
      return this;
   }

   /// <inheritdoc />
   public IQueryBuilder<T> AddCondition(IGridifyFiltering condition)
   {
      if (condition.Filter != null)
         _conditionList.Add(condition.Filter);
      return this;
   }

   /// <inheritdoc />
   public IQueryBuilder<T> AddQuery(IGridifyQuery gridifyQuery)
   {
      if (gridifyQuery.Filter != null)
         AddCondition(gridifyQuery.Filter);

      if (!string.IsNullOrEmpty(gridifyQuery.OrderBy))
         AddOrderBy(gridifyQuery.OrderBy!);

      if (gridifyQuery.PageSize == 0) gridifyQuery.PageSize = GridifyGlobalConfiguration.DefaultPageSize;
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
   public IQueryBuilder<T> AddMap(string from, Expression<Func<T, int, object?>> to, Func<string, object>? convertor = null, bool overwrite = true)
   {
      _mapper ??= new GridifyMapper<T>(true);
      _mapper.AddMap(from, to, convertor, overwrite);
      return this;
   }

   /// <inheritdoc />
   public IQueryBuilder<T> AddMap(string from, Expression<Func<T, string, object?>> to, Func<string, object>? convertor = null, bool overwrite = true)
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
   public bool IsValid()
   {
      var isValid = true;
      _mapper ??= new GridifyMapper<T>(true);
      try
      {
         if (_conditionList.Count > 0)
         {
            var gqList = _conditionList.Select(q => new GridifyQuery() { Filter = q }).Cast<IGridifyFiltering>();
            isValid = isValid && gqList.All(q => q.IsValid(_mapper));
         }

         if (!string.IsNullOrWhiteSpace(_orderBy))
         {
            IGridifyOrdering gq = new GridifyQuery() { OrderBy = _orderBy };
            isValid = isValid && gq.IsValid(_mapper);
         }
      }
      catch (Exception)
      {
         return false;
      }

      return isValid;
   }

   /// <inheritdoc />
   public Expression<Func<T, bool>> BuildFilteringExpression()
   {
      if (_conditionList.Count == 0)
         return _ => true;

      var _conditions = _conditionList.Select(ConvertConditionToExpression).ToList();
      return (_conditions.Aggregate(null, (LambdaExpression? x, LambdaExpression y)
         => x is null ? y : x.And(y)) as Expression<Func<T, bool>>)!;
   }

   /// <inheritdoc />
   public Func<IQueryable<T>, bool> BuildEvaluator()
   {
      var _conditions = _conditionList.Select(ConvertConditionToExpression).ToList();
      return collection =>
      {
         return _conditions.Count == 0 ||
                _conditions.Aggregate(true, (current, expression) =>
                   current & collection.Any(expression));
      };
   }

   /// <inheritdoc />
   public Func<IEnumerable<T>, bool> BuildCompiledEvaluator()
   {
      var _conditions = _conditionList.Select(ConvertConditionToExpression).ToList();
      var compiledCond = _conditions.Select(q => q.Compile()).ToList();
      var length = _conditions.Count;
      return collection =>
      {
         return length == 0 ||
                compiledCond.Aggregate(true, (current, expression)
                   => current && collection.Any(expression));
      };
   }

   /// <inheritdoc />
   public bool Evaluate(IQueryable<T> query)
   {
      return BuildEvaluator()(query);
   }

   /// <inheritdoc />
   public bool Evaluate(IEnumerable<T> collection)
   {
      return BuildCompiledEvaluator()(collection);
   }

   /// <inheritdoc />
   public IQueryable<T> Build(IQueryable<T> context)
   {
      var query = context;

      if (_conditionList.Count > 0)
         query = query.Where(BuildFilteringExpression());

      if (!string.IsNullOrEmpty(_orderBy))
         query = query.ApplyOrdering(_orderBy, _mapper);

      if (_paging.HasValue)
         query = query.Skip(_paging.Value.page * _paging.Value.pageSize).Take(_paging.Value.pageSize);

      return query;
   }

   /// <inheritdoc />
   public Func<IQueryable<T>, IQueryable<T>> Build()
   {
      return Build;
   }

   /// <inheritdoc />
   public Func<IEnumerable<T>, IEnumerable<T>> BuildCompiled()
   {
      var compiled = BuildFilteringExpression().Compile();
      return collection =>
      {
         if (_conditionList.Count > 0)
            collection = collection.Where(compiled);

         if (!string.IsNullOrEmpty(_orderBy)) // TODO: this also should be compiled
            collection = collection.AsQueryable().ApplyOrdering(_orderBy, _mapper);

         if (_paging.HasValue)
            collection = collection.Skip(_paging.Value.page * _paging.Value.pageSize).Take(_paging.Value.pageSize);

         return collection;
      };
   }

   /// <inheritdoc />
   public IEnumerable<T> Build(IEnumerable<T> collection)
   {
      if (_conditionList.Count > 0)
         collection = collection.Where(BuildFilteringExpression().Compile());

      if (!string.IsNullOrEmpty(_orderBy))
         collection = collection.AsQueryable().ApplyOrdering(_orderBy, _mapper);

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

   public Func<IEnumerable<T>, Paging<T>> BuildWithPagingCompiled()
   {
      var compiled = BuildFilteringExpression().Compile();
      return collection =>
      {
         if (_conditionList.Count > 0)
            collection = collection.Where(compiled);

         if (!string.IsNullOrEmpty(_orderBy)) // TODO: this also should be compiled
            collection = collection.AsQueryable().ApplyOrdering(_orderBy, _mapper);

         var result = collection.ToList();
         var count = result.Count();

         return _paging.HasValue
            ? new Paging<T>(count, result.Skip(_paging.Value.page * _paging.Value.pageSize).Take(_paging.Value.pageSize))
            : new Paging<T>(count, result);
      };
   }


   /// <inheritdoc />
   public QueryablePaging<T> BuildWithQueryablePaging(IQueryable<T> collection)
   {
      var query = collection;
      if (_conditionList.Count > 0)
         query = query.Where(BuildFilteringExpression());

      if (!string.IsNullOrEmpty(_orderBy))
         query = query.ApplyOrdering(_orderBy, _mapper);

      var count = query.Count();

      if (_paging.HasValue)
         query = query.Skip(_paging.Value.page * _paging.Value.pageSize).Take(_paging.Value.pageSize);

      return new QueryablePaging<T>(count, query);
   }

   private Expression<Func<T, bool>> ConvertConditionToExpression(string condition)
   {
      var syntaxTree = SyntaxTree.Parse(condition, GridifyGlobalConfiguration.CustomOperators.Operators);

      if (syntaxTree.Diagnostics.Any())
         throw new GridifyFilteringException(syntaxTree.Diagnostics.Last());

      return syntaxTree.CreateQuery(_mapper);
   }
}
