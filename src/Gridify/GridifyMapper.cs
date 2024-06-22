#nullable enable
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace Gridify;

public class GridifyMapper<T> : IGridifyMapper<T>
{
   public GridifyMapperConfiguration Configuration { get; protected set; }
   private readonly List<IGMap<T>> _mappings;

   public GridifyMapper(bool autoGenerateMappings = false, ushort maxNestingDepth = 0)
   {
      Configuration = new GridifyMapperConfiguration();
      _mappings = new List<IGMap<T>>();

      if (autoGenerateMappings)
         GenerateMappings();
   }

   public GridifyMapper(GridifyMapperConfiguration configuration, bool autoGenerateMappings = false)
   {
      Configuration = configuration;
      _mappings = new List<IGMap<T>>();

      if (autoGenerateMappings)
         GenerateMappings();
   }

   public GridifyMapper(Action<GridifyMapperConfiguration> configuration, bool autoGenerateMappings = false)
   {
      Configuration = new GridifyMapperConfiguration();
      configuration.Invoke(Configuration);
      _mappings = new List<IGMap<T>>();

      if (autoGenerateMappings)
         GenerateMappings();
   }

   public IGridifyMapper<T> AddMap(string from, Func<string, object>? convertor = null!, bool overrideIfExists = true)
   {
      if (!overrideIfExists && HasMap(from))
         throw new GridifyMapperException($"Duplicate Key. the '{from}' key already exists");

      Expression<Func<T, object>> to;
      try
      {
         var expression = CreateExpression(from);
         if (expression is null)
            return this;
         to = expression;
      }
      catch (Exception)
      {
         throw new GridifyMapperException($"Property '{from}' not found.");
      }

      RemoveMap(from);
      _mappings.Add(new GMap<T>(from, to!, convertor));
      return this;
   }

   /// <inheritdoc />
   public IGridifyMapper<T> GenerateMappings()
   {
      GenerateMappingsRecursive(typeof(T), "", 0, 0);
      return this;
   }

   /// <inheritdoc />
   public IGridifyMapper<T> GenerateMappings(ushort maxNestingDepth)
   {
      GenerateMappingsRecursive(typeof(T), "", maxNestingDepth, 0);
      return this;
   }

   private void GenerateMappingsRecursive(Type type, string prefix, ushort maxNestingDepth, ushort currentDepth)
   {
      foreach (var item in type.GetProperties())
      {
         var propertyName = char.ToLowerInvariant(item.Name[0]) + item.Name.Substring(1); // camel-case name
         var fullName = string.IsNullOrEmpty(prefix) ? propertyName : $"{prefix}.{propertyName}";

         // Skip classes if nestingLevel is exceeded
         if (item.PropertyType.IsClass && item.PropertyType != typeof(string))
         {
            if (currentDepth >= maxNestingDepth)
            {
               continue;
            }
            // If nestingLevel is not exceeded and the property is a class, recursively generate mappings
            GenerateMappingsRecursive(item.PropertyType, fullName, maxNestingDepth, (ushort)(currentDepth + 1));
            continue;
         }

         _mappings.Add(new GMap<T>(fullName, CreateExpression(fullName)!));
      }
   }

   public IGridifyMapper<T> AddMap(string from, Expression<Func<T, object?>> to, Func<string, object>? convertor = null!,
      bool overrideIfExists = true)
   {
      if (!overrideIfExists && HasMap(from))
         throw new GridifyMapperException($"Duplicate Key. the '{from}' key already exists");

      RemoveMap(from);
      _mappings.Add(new GMap<T>(from, to, convertor));
      return this;
   }

   public IGridifyMapper<T> AddMap(string from, Expression<Func<T, int, object?>> to, Func<string, object>? convertor = null!,
      bool overrideIfExists = true)
   {
      if (!overrideIfExists && HasMap(from))
         throw new GridifyMapperException($"Duplicate Key. the '{from}' key already exists");

      RemoveMap(from);
      _mappings.Add(new GMap<T>(from, to, convertor));
      return this;
   }

   public IGridifyMapper<T> AddMap(string from, Expression<Func<T, string, object?>> to, Func<string, object>? convertor = null!,
      bool overrideIfExists = true)
   {
      if (!overrideIfExists && HasMap(from))
         throw new GridifyMapperException($"Duplicate Key. the '{from}' key already exists");

      RemoveMap(from);
      _mappings.Add(new GMap<T>(from, to, convertor));
      return this;
   }

   public IGridifyMapper<T> AddMap<TSubKey>(string from, Expression<Func<T, TSubKey, object?>> to, Func<string, object>? convertor = null!,
      bool overrideIfExists = true)
   {
      if (!overrideIfExists && HasMap(from))
         throw new GridifyMapperException($"Duplicate Key. the '{from}' key already exists");

      RemoveMap(from);
      _mappings.Add(new GMap<T>(from, to, convertor)); // how to resolve this error??!
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

   public Expression<Func<T, object>> GetExpression(string key)
   {
      var expression = Configuration.CaseSensitive
         ? _mappings.FirstOrDefault(q => key.Equals(q.From))?.To
         : _mappings.FirstOrDefault(q => key.Equals(q.From, StringComparison.InvariantCultureIgnoreCase))?.To;
      if (expression == null)
         throw new GridifyMapperException($"Mapping Key `{key}` not found.");
      return expression as Expression<Func<T, object>> ?? throw new GridifyMapperException($"Expression fir the `{key}` not found.");
   }

   public IEnumerable<IGMap<T>> GetCurrentMaps()
   {
      return _mappings;
   }

   /// <summary>
   /// Converts current mappings to a comma seperated list of map names.
   /// eg, field1,field2,field3
   /// </summary>
   /// <returns>a comma seperated string</returns>
   public override string ToString() => string.Join(",", _mappings.Select(q => q.From));

   internal static Expression<Func<T, object>>? CreateExpression(string from) // TODO: handle possible nulls
   {
      // Param_x =>
      var parameter = Expression.Parameter(typeof(T), "__" + typeof(T).Name);
      // Param_x.Name, Param_x.yyy.zz.xx
      var mapProperty = from.Split('.').Aggregate<string, Expression>(parameter, Expression.Property);

      if (!IsListOrArrayOfPrimitivesOrString(mapProperty.Type, out var genericType))
      {
         // (object)Param_x.Name
         var convertedExpression = Expression.Convert(mapProperty, typeof(object));
         // Param_x => (object)Param_x.Name
         return Expression.Lambda<Func<T, object>>(convertedExpression, parameter);
      }

      var selectMethod = GetSelectMethod([genericType!, genericType!]);
      if (selectMethod is null)
         return null;
      var predicateParameter = Expression.Parameter(genericType!);
      var predicate = Expression.Lambda(predicateParameter, predicateParameter);
      //  Param_x.Name.Select(fc => fc)
      var body = Expression.Call(selectMethod, mapProperty, predicate);
      return Expression.Lambda<Func<T, object>>(body, parameter);
   }

   private static MethodInfo? GetSelectMethod(Type[] type)
   {
      return typeof(Enumerable).GetMethods().First(m => m.Name == "Select").MakeGenericMethod(type);
   }

   private static bool IsListOrArrayOfPrimitivesOrString(Type type, out Type? itemType)
   {
      itemType = null;
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
      {
         var arguments = type.GetGenericArguments();
         if (arguments.Length != 1 || !IsPrimitiveOrKnownType(arguments[0])) return false;

         itemType = arguments[0];
         return true;
      }
      if (!type.IsArray) return false;

      var elementType = type.GetElementType();
      if (elementType == null || !IsPrimitiveOrKnownType(elementType)) return false;
      itemType = elementType;
      return true;
   }

   private static bool IsPrimitiveOrKnownType(Type type)
   {
      // Primitive types in C# include: int, float, double, char, bool, etc.
      // Also consider the known primitive types and string explicitly
      return type.IsPrimitive ||
             (type == typeof(string)) ||
             (type == typeof(decimal)) ||
             (type == typeof(DateTime)) ||
             (type == typeof(Guid));
   }

}
