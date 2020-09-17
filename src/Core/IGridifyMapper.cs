using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gridify
{
   public interface IGridifyMapper<T>
   {
      IGridifyMapper<T> AddMap(string from, Expression<Func<T, object>> to, Func<string, object> convertor = null, bool overrideIfExists = true);
      IGridifyMapper<T> AddMap(GMap<T> gMap, bool overrideIfExists = true);
      IGridifyMapper<T> GenerateMappings();
      IGridifyMapper<T> RemoveMap(string propertyName);
      IGridifyMapper<T> RemoveMap(GMap<T> gMap);
      Expression<Func<T, object>> GetExpression(string from);
      GMap<T> GetGMap(string from);
      bool HasMap(string key);
      bool CaseSensitive {get;} 
      IEnumerable<GMap<T>> GetCurrentMaps();
   }
}