#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Gridify
{
   public class GridifyMapper<T> : IGridifyMapper<T>
   {
      public GridifyMapperConfiguration Configuration { get; }
      private readonly List<IGMap<T>> _mappings;

      public GridifyMapper()
      {
         Configuration = new GridifyMapperConfiguration();
         _mappings = new List<IGMap<T>>();
      }

      public GridifyMapper(GridifyMapperConfiguration configuration)
      {
         Configuration = configuration;
         _mappings = new List<IGMap<T>>();
      }

      public GridifyMapper(Action<GridifyMapperConfiguration> configuration)
      {
         Configuration = new GridifyMapperConfiguration();
         configuration.Invoke(Configuration);
         _mappings = new List<IGMap<T>>();
      }

      public IGridifyMapper<T> GenerateMappings()
      {
         foreach (var item in typeof(T).GetProperties())
         {
            // skip classes
            if (item.PropertyType.IsClass && item.PropertyType != typeof(string))
               continue;

            var name = char.ToLowerInvariant(item.Name[0]) + item.Name.Substring(1); // camel-case name
            _mappings.Add(new GMap<T>(name, CreateExpression(item.Name)!));
         }

         return this;
      }

      public IGridifyMapper<T> AddMap(string from, Expression<Func<T, object?>> to, Func<string, object>? convertor = null!,
         bool overrideIfExists = true)
      {
         if (!overrideIfExists && HasMap(from))
            throw new GridifyMapperException($"Duplicate Key. the '{from}' key already exists");

         RemoveMap(from);
         var isNested = Regex.IsMatch(to.ToString(), @"\.Select\s*\(");
         _mappings.Add(new GMap<T>(from, to, convertor, isNested));
         return this;
      }

      public IGridifyMapper<T> AddMap(string from, Expression<Func<T, int, object?>> to, Func<string, object>? convertor = null!,
         bool overrideIfExists = true)
      {
         if (!overrideIfExists && HasMap(from))
            throw new GridifyMapperException($"Duplicate Key. the '{from}' key already exists");

         RemoveMap(from);
         var isNested = Regex.IsMatch(to.ToString(), @"\.Select\s*\(");
         _mappings.Add(new GMap<T>(from, to, convertor, isNested));
         return this;
      }

      public IGridifyMapper<T> AddMap(IGMap<T> gMap, bool overrideIfExists = true)
      {
         if (!overrideIfExists && HasMap(gMap.From))
            throw new GridifyMapperException($"Duplicate Key. the '{gMap.From}' key already exists");

         RemoveMap(gMap.From);
         _mappings.Add(gMap);
         return this;
      }

      public IGridifyMapper<T> RemoveMap(string from)
      {
         var map = GetGMap(from);
         if (map != null)
            _mappings.Remove(map);
         return this;
      }

      public IGridifyMapper<T> RemoveMap(IGMap<T> gMap)
      {
         _mappings.Remove(gMap);
         return this;
      }

      public bool HasMap(string from)
      {
         return Configuration.CaseSensitive
            ? _mappings.Any(q => q.From == from)
            : _mappings.Any(q => from.Equals(q.From, StringComparison.InvariantCultureIgnoreCase));
      }

      public IGMap<T>? GetGMap(string from)
      {
         return Configuration.CaseSensitive
            ? _mappings.FirstOrDefault(q => from.Equals(q.From))
            : _mappings.FirstOrDefault(q => from.Equals(q.From, StringComparison.InvariantCultureIgnoreCase));
      }

      public LambdaExpression GetLambdaExpression(string key)
      {
         var expression = Configuration.CaseSensitive
            ? _mappings.FirstOrDefault(q => key.Equals(q.From))?.To
            : _mappings.FirstOrDefault(q => key.Equals(q.From, StringComparison.InvariantCultureIgnoreCase))?.To;
         if (expression == null)
            throw new GridifyMapperException($"Mapping Key `{key}` not found.");
         return expression!;
      }
      
      public Expression<Func<T,object>> GetExpression(string key)
      {
         var expression = Configuration.CaseSensitive
            ? _mappings.FirstOrDefault(q => key.Equals(q.From))?.To
            : _mappings.FirstOrDefault(q => key.Equals(q.From, StringComparison.InvariantCultureIgnoreCase))?.To;
         if (expression == null)
            throw new GridifyMapperException($"Mapping Key `{key}` not found.");
         return expression as Expression<Func<T,object>> ?? throw new GridifyMapperException($"Expression fir the `{key}` not found.");
      }

      public IEnumerable<IGMap<T>> GetCurrentMaps()
      {
         return _mappings;
      }

      /// <summary>
      /// Converts current mappings to a comma seperated list of map names.
      /// eg, filed1,field2,field3 
      /// </summary>
      /// <returns>a comma seperated string</returns>
      public override string ToString() => string.Join(",", _mappings.Select(q => q.From));

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