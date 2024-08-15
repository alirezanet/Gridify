using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gridify;

public interface IGridifyMapper<T>
{
   IGridifyMapper<T> AddMap(string from, Expression<Func<T, object?>> to, Func<string, object>? convertor = null, bool overrideIfExists = true);

   IGridifyMapper<T> AddMap(string from, Expression<Func<T, int, object?>> to, Func<string, object>? convertor = null!,
      bool overrideIfExists = true);
   IGridifyMapper<T> AddMap(string from, Expression<Func<T, string, object?>> to, Func<string, object>? convertor = null!,
      bool overrideIfExists = true);
   IGridifyMapper<T> AddMap<TSubKey>(string from, Expression<Func<T, TSubKey, object?>> to, Func<string, object>? convertor = null!,
      bool overrideIfExists = true);

   IGridifyMapper<T> AddMap(IGMap<T> gMap, bool overrideIfExists = true);
   IGridifyMapper<T> AddMap(string from, Func<string, object>? convertor = null!, bool overrideIfExists = true);

   /// <summary>
   /// Generates property mappings for the specified class type <typeparamref name="T"/>.
   /// </summary>
   /// <returns>An instance of <see cref="IGridifyMapper{T}"/> with property mappings.</returns>
   IGridifyMapper<T> GenerateMappings();

   /// <summary>
   /// Generates property mappings for the specified class type <typeparamref name="T"/> with control over nesting depth.
   /// </summary>
   /// <param name="maxNestingDepth">The maximum nesting depth for recursive mapping generation. Use 0 for no nesting.</param>
   /// <returns>An instance of <see cref="IGridifyMapper{T}"/> with property mappings.</returns>
   IGridifyMapper<T> GenerateMappings(ushort maxNestingDepth)
#if NETSTANDARD2_0  // default implementation To avoid breaking changes,TODO (V3): should be removed in v3
      ;
#else
   {

      return GenerateMappings();
   }
#endif

   IGridifyMapper<T> RemoveMap(string propertyName);
   IGridifyMapper<T> RemoveMap(IGMap<T> gMap);
   LambdaExpression GetLambdaExpression(string from);
   Expression<Func<T, object>> GetExpression(string key);
   IGMap<T>? GetGMap(string from);
   bool HasMap(string key);
   public GridifyMapperConfiguration Configuration { get; }
   IEnumerable<IGMap<T>> GetCurrentMaps();
   void ClearMappings();
}
