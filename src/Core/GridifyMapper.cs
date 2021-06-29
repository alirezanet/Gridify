#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gridify
{
   public partial class GridifyMapper<T> : IGridifyMapper<T>
   {
      private readonly HashSet<IGMap<T>> _mappings;

      public GridifyMapper(bool caseSensitive = false)
      {
         CaseSensitive = caseSensitive;
         _mappings = new HashSet<IGMap<T>>();
      }

      public bool CaseSensitive { get; }

      public IGridifyMapper<T> GenerateMappings()
      {
         foreach (var item in typeof(T).GetProperties())
         {
            var name = char.ToLowerInvariant(item.Name[0]) + item.Name.Substring(1); // camel-case name

            // add to mapper object
            _mappings.Add(new GMap<T>(name, CreateExpression(item.Name)));
         }

         return this;
      }

      public IGridifyMapper<T> AddMap(string from, Expression<Func<T, object>> to, Func<string, object> convertor = null!,
         bool overrideIfExists = true)
      {
         if (!overrideIfExists && HasMap(from))
            throw new Exception($"Duplicate Key. the '{from}' key already exists");

         RemoveMap(from);
         _mappings.Add(new GMap<T>(from, to, convertor));
         return this;
      }

      public IGridifyMapper<T> AddMap(IGMap<T> gMap, bool overrideIfExists = true)
      {
         if (!overrideIfExists && HasMap(gMap.From))
            throw new Exception($"Duplicate Key. the '{gMap.From}' key already exists");

         RemoveMap(gMap.From);
         _mappings.Add(gMap);
         return this;
      }

      public IGridifyMapper<T> RemoveMap(string from)
      {
         _ = CaseSensitive
            ? _mappings.RemoveWhere(q => from.Equals(q.From))
            : _mappings.RemoveWhere(q => from.Equals(q.From, StringComparison.InvariantCultureIgnoreCase));
         return this;
      }

      public IGridifyMapper<T> RemoveMap(IGMap<T> gMap)
      {
         _mappings.Remove(gMap);
         return this;
      }

      public bool HasMap(string from)
      {
         return CaseSensitive
            ? _mappings.Any(q => q.From == from)
            : _mappings.Any(q => from.Equals(q.From, StringComparison.InvariantCultureIgnoreCase));
      }

      public IGMap<T>? GetGMap(string from)
      {
         return CaseSensitive
            ? _mappings.FirstOrDefault(q => from.Equals(q.From))
            : _mappings.FirstOrDefault(q => from.Equals(q.From, StringComparison.InvariantCultureIgnoreCase));
      }

      public Expression<Func<T, object>> GetExpression(string key)
      {
         var expression = CaseSensitive
            ? _mappings.FirstOrDefault(q => key.Equals(q.From))?.To
            : _mappings.FirstOrDefault(q => key.Equals(q.From, StringComparison.InvariantCultureIgnoreCase))?.To;
         if (expression == null)
            throw new GridifyMapperException($"Mapping Key `{key}` not found.");
         return expression;
      }

      public IEnumerable<IGMap<T>> GetCurrentMaps()
      {
         return _mappings.AsEnumerable();
      }

      private static Expression<Func<T, object>> CreateExpression(string from)
      {
         // x =>
         var parameter = Expression.Parameter(typeof(T));
         // x.Name
         var mapProperty = Expression.Property(parameter, from);
         // (object)x.Name
         var convertedExpression = Expression.Convert(mapProperty, typeof(object));
         // x => (object)x.Name
         return Expression.Lambda<Func<T, object>>(convertedExpression, parameter);
      }
   }
}