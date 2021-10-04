using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gridify
{
   public interface IGridifyMapper<T>
   {
      IGridifyMapper<T> AddMap(string from, Expression<Func<T, object?>> to, Func<string, object>? convertor = null, bool overrideIfExists = true);

      IGridifyMapper<T> AddMap(string from, Expression<Func<T, int, object?>> to, Func<string, object>? convertor = null!,
         bool overrideIfExists = true);

      IGridifyMapper<T> AddMap(IGMap<T> gMap, bool overrideIfExists = true);
      IGridifyMapper<T> GenerateMappings();
      IGridifyMapper<T> RemoveMap(string propertyName);
      IGridifyMapper<T> RemoveMap(IGMap<T> gMap);
      LambdaExpression GetLambdaExpression(string from);
      Expression<Func<T, object?>> GetExpression(string key);
      IGMap<T>? GetGMap(string from);
      bool HasMap(string key);
      public GridifyMapperConfiguration Configuration { get; }
      IEnumerable<IGMap<T>> GetCurrentMaps();
   }
}