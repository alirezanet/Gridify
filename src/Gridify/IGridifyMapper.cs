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
   /// Adds a composite mapping that combines multiple property expressions with OR logic.
   /// </summary>
   /// <param name="from">The field name to map from</param>
   /// <param name="convertor">Optional value converter function</param>
   /// <param name="expressions">Multiple expressions that will be combined with OR logic</param>
   /// <returns>An instance of <see cref="IGridifyMapper{T}"/> for method chaining</returns>
   IGridifyMapper<T> AddCompositeMap(string from, Func<string, object>? convertor, params Expression<Func<T, object?>>[] expressions);

   /// <summary>
   /// Adds a composite mapping that combines multiple property expressions with OR logic.
   /// </summary>
   /// <param name="from">The field name to map from</param>
   /// <param name="expressions">Multiple expressions that will be combined with OR logic</param>
   /// <returns>An instance of <see cref="IGridifyMapper{T}"/> for method chaining</returns>
   IGridifyMapper<T> AddCompositeMap(string from, params Expression<Func<T, object?>>[] expressions);

   /// <summary>
   /// Reuses mappings from a nested object's mapper by composing expressions.
   /// Merges nested mappings directly without a prefix.
   /// </summary>
   /// <typeparam name="TProperty">The type of the nested property</typeparam>
   /// <param name="propertyExpression">Expression pointing to the nested property (e.g., x => x.Address)</param>
   /// <param name="nestedMapper">The mapper containing mappings for the nested type</param>
   /// <param name="overrideIfExists">Whether to override existing mappings with the same key</param>
   /// <returns>An instance of <see cref="IGridifyMapper{T}"/> for method chaining</returns>
   /// <example>
   /// <code>
   /// var addressMapper = new GridifyMapper&lt;Address&gt;()
   ///     .AddMap("city", x => x.City)
   ///     .AddMap("country", x => x.Country);
   /// 
   /// var userMapper = new GridifyMapper&lt;User&gt;()
   ///     .AddMap("email", x => x.Email)
   ///     .AddNestedMapper(x => x.Address, addressMapper);
   /// // Now supports: "city=London", "country=UK"
   /// </code>
   /// </example>
   IGridifyMapper<T> AddNestedMapper<TProperty>(
      Expression<Func<T, TProperty>> propertyExpression,
      IGridifyMapper<TProperty> nestedMapper,
      bool overrideIfExists = true);

   /// <summary>
   /// Reuses mappings from a nested object's mapper by composing expressions with a prefix.
   /// </summary>
   /// <typeparam name="TProperty">The type of the nested property</typeparam>
   /// <param name="prefix">Prefix to prepend to nested mapping keys (e.g., "location" creates "location.city")</param>
   /// <param name="propertyExpression">Expression pointing to the nested property (e.g., x => x.Address)</param>
   /// <param name="nestedMapper">The mapper containing mappings for the nested type</param>
   /// <param name="overrideIfExists">Whether to override existing mappings with the same key</param>
   /// <returns>An instance of <see cref="IGridifyMapper{T}"/> for method chaining</returns>
   /// <example>
   /// <code>
   /// var addressMapper = new GridifyMapper&lt;Address&gt;()
   ///     .AddMap("city", x => x.City)
   ///     .AddMap("country", x => x.Country);
   /// 
   /// var companyMapper = new GridifyMapper&lt;Company&gt;()
   ///     .AddMap("name", x => x.Name)
   ///     .AddNestedMapper("location", x => x.Address, addressMapper);
   /// // Now supports: "location.city=London", "location.country=UK"
   /// </code>
   /// </example>
   IGridifyMapper<T> AddNestedMapper<TProperty>(
      string prefix,
      Expression<Func<T, TProperty>> propertyExpression,
      IGridifyMapper<TProperty> nestedMapper,
      bool overrideIfExists = true);

   /// <summary>
   /// Reuses mappings from a custom mapper class for a nested object by composing expressions.
   /// Merges mappings directly without a prefix.
   /// </summary>
   /// <typeparam name="TProperty">The type of the nested property</typeparam>
   /// <typeparam name="TMapper">The mapper class type that implements IGridifyMapper&lt;TProperty&gt;</typeparam>
   /// <param name="propertyExpression">Expression pointing to the nested property (e.g., x => x.Address)</param>
   /// <param name="overrideIfExists">Whether to override existing mappings with the same key</param>
   /// <returns>An instance of <see cref="IGridifyMapper{T}"/> for method chaining</returns>
   /// <example>
   /// <code>
   /// public class AddressGridifyMapper : GridifyMapper&lt;Address&gt;
   /// {
   ///     public AddressGridifyMapper()
   ///     {
   ///         AddMap("city", q => q.City);
   ///         AddMap("country", q => q.Country);
   ///     }
   /// }
   /// 
   /// var userMapper = new GridifyMapper&lt;User&gt;()
   ///     .AddMap("email", x => x.Email)
   ///     .AddNestedMapper&lt;Address, AddressGridifyMapper&gt;(x => x.Address);
   /// // Uses AddressGridifyMapper and merges mappings: "city=London", "country=UK"
   /// </code>
   /// </example>
   IGridifyMapper<T> AddNestedMapper<TProperty, TMapper>(
      Expression<Func<T, TProperty>> propertyExpression,
      bool overrideIfExists = true)
      where TMapper : IGridifyMapper<TProperty>, new();

   /// <summary>
   /// Reuses mappings from a custom mapper class for a nested object by composing expressions with a prefix.
   /// </summary>
   /// <typeparam name="TProperty">The type of the nested property</typeparam>
   /// <typeparam name="TMapper">The mapper class type that implements IGridifyMapper&lt;TProperty&gt;</typeparam>
   /// <param name="prefix">Prefix to prepend to nested mapping keys (e.g., "location" creates "location.city")</param>
   /// <param name="propertyExpression">Expression pointing to the nested property (e.g., x => x.Address)</param>
   /// <param name="overrideIfExists">Whether to override existing mappings with the same key</param>
   /// <returns>An instance of <see cref="IGridifyMapper{T}"/> for method chaining</returns>
   /// <example>
   /// <code>
   /// public class AddressGridifyMapper : GridifyMapper&lt;Address&gt;
   /// {
   ///     public AddressGridifyMapper()
   ///     {
   ///         AddMap("city", q => q.City);
   ///         AddMap("country", q => q.Country);
   ///     }
   /// }
   /// 
   /// var companyMapper = new GridifyMapper&lt;Company&gt;()
   ///     .AddMap("name", x => x.Name)
   ///     .AddNestedMapper&lt;Address, AddressGridifyMapper&gt;("location", x => x.Address);
   /// // Uses AddressGridifyMapper with prefix: "location.city=London", "location.country=UK"
   /// </code>
   /// </example>
   IGridifyMapper<T> AddNestedMapper<TProperty, TMapper>(
      string prefix,
      Expression<Func<T, TProperty>> propertyExpression,
      bool overrideIfExists = true)
      where TMapper : IGridifyMapper<TProperty>, new();

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
   IEnumerable<IGMap<T>> GetCurrentMapsByType<TTarget>();
   IEnumerable<IGMap<T>> GetCurrentMapsByType(HashSet<Type> targetTypes);
   void ClearMappings();
}
