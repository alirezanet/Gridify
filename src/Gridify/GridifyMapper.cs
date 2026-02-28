using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gridify.Reflection;
using Gridify.Syntax;

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
         GenerateMappings(maxNestingDepth);
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
         to = CreateExpression(from);
         ;
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

         if (item.PropertyType.IsComplexTypeCollection(out var genericType))
         {
            if (currentDepth >= maxNestingDepth)
            {
               continue;
            }

            GenerateMappingsRecursive(genericType!, fullName, maxNestingDepth, (ushort)(currentDepth + 1));
            continue;
         }

         // Skip classes if nestingLevel is exceeded
         if (item.PropertyType.IsClass && item.PropertyType != typeof(string) && !item.PropertyType.IsSimpleTypeCollection(out _))
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

   public IGridifyMapper<T> AddCompositeMap(string from, params Expression<Func<T, object?>>[] expressions)
   {
      return AddCompositeMap(from, null, expressions);
   }

   public IGridifyMapper<T> AddCompositeMap(string from, Func<string, object>? convertor, params Expression<Func<T, object?>>[] expressions)
   {
      if (expressions == null || expressions.Length == 0)
         throw new GridifyMapperException("At least one expression must be provided");

      RemoveMap(from);
      _mappings.Add(new CompositeGMap<T>(from, convertor, expressions));
      return this;
   }

   public IGridifyMapper<T> AddNestedMapper<TProperty>(
      string prefix,
      Expression<Func<T, TProperty>> propertyExpression,
      IGridifyMapper<TProperty> nestedMapper,
      bool overrideIfExists = true)
   {
      if (string.IsNullOrEmpty(prefix))
         throw new ArgumentNullException(nameof(prefix));
      if (propertyExpression == null)
         throw new ArgumentNullException(nameof(propertyExpression));
      if (nestedMapper == null)
         throw new ArgumentNullException(nameof(nestedMapper));

      return AddNestedMapperInternal(propertyExpression, nestedMapper, prefix, overrideIfExists);
   }

   public IGridifyMapper<T> AddNestedMapper<TProperty>(
      Expression<Func<T, TProperty>> propertyExpression,
      IGridifyMapper<TProperty> nestedMapper,
      bool overrideIfExists = true)
   {
      if (propertyExpression == null)
         throw new ArgumentNullException(nameof(propertyExpression));
      if (nestedMapper == null)
         throw new ArgumentNullException(nameof(nestedMapper));

      // Extract property name from expression to use as prefix
      var prefix = ExtractPropertyNameInCamelCase(propertyExpression);
      return AddNestedMapperInternal(propertyExpression, nestedMapper, prefix, overrideIfExists);
   }

   private IGridifyMapper<T> AddNestedMapperInternal<TProperty>(
      Expression<Func<T, TProperty>> propertyExpression,
      IGridifyMapper<TProperty> nestedMapper,
      string prefix,
      bool overrideIfExists)
   {

      // Get the parameter from the parent expression
      var parentParameter = propertyExpression.Parameters[0];

      // Iterate through all mappings in the nested mapper
      foreach (var nestedMap in nestedMapper.GetCurrentMaps())
      {
         var compositeKey = $"{prefix}.{nestedMap.From}";

         // Get the nested expression
         var nestedExpression = nestedMap.To;

         // Compose the expressions: parent property access + nested property access
         Expression composedBody;

         if (nestedExpression is Expression<Func<TProperty, object>> typedNestedExpr)
         {
            // Replace the nested parameter with the parent's property access
            var propertyAccess = propertyExpression.Body;

            // If the body is a conversion (UnaryExpression), unwrap it
            if (propertyAccess is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert)
            {
               propertyAccess = unaryExpr.Operand;
            }

            composedBody = new ReplaceExpressionVisitor(typedNestedExpr.Parameters[0], propertyAccess)
               .Visit(typedNestedExpr.Body)!;
         }
         else
         {
            // Handle non-generic lambda expressions
            var propertyAccess = propertyExpression.Body;

            // If the body is a conversion (UnaryExpression), unwrap it
            if (propertyAccess is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert)
            {
               propertyAccess = unaryExpr.Operand;
            }

            composedBody = new ReplaceExpressionVisitor(nestedExpression.Parameters[0], propertyAccess)
               .Visit(nestedExpression.Body)!;
         }

         // Create the composed expression
         var composedExpression = Expression.Lambda<Func<T, object>>(composedBody, parentParameter);

         // Handle CompositeGMap specially
         if (nestedMap is CompositeGMap<TProperty> compositeMap)
         {
            // For composite maps, we need to compose all expressions
            var composedExpressions = new List<Expression<Func<T, object?>>>();

            foreach (var expr in compositeMap.Expressions)
            {
               var propertyAccess = propertyExpression.Body;
               if (propertyAccess is UnaryExpression unaryExpr && unaryExpr.NodeType == ExpressionType.Convert)
               {
                  propertyAccess = unaryExpr.Operand;
               }

               var composedExpr = new ReplaceExpressionVisitor(expr.Parameters[0], propertyAccess)
                  .Visit(expr.Body)!;

               composedExpressions.Add(Expression.Lambda<Func<T, object?>>(composedExpr, parentParameter));
            }

            AddCompositeMap(compositeKey, nestedMap.Convertor, composedExpressions.ToArray());
         }
         else
         {
            // Skip if the map already exists and overrideIfExists is false
            if (!overrideIfExists && HasMap(compositeKey))
            {
               continue;
            }

            // Add the composed mapping - cast to Expression<Func<T, object?>> to satisfy nullability
            AddMap(compositeKey, (Expression<Func<T, object?>>)composedExpression, nestedMap.Convertor, overrideIfExists);
         }
      }

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

   public void ClearMappings()
   {
      _mappings.Clear();
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

      var exception = new GridifyMapperException($"Expression for the `{key}` not found.");
      ;

      // handle 2-parameter mapping: (target, keyParam) => ...
      if (expression.Parameters.Count == 2)
      {
         var targetParam = expression.Parameters[0];
         var keyParam = expression.Parameters[1];

         var constKey = Expression.Constant(key, typeof(string));

         // replace `keyParam` with `"keyValue"`
         var bodyWithKey = new ReplaceExpressionVisitor(keyParam, constKey).Visit(expression.Body);

         return Expression.Lambda<Func<T, object>>(bodyWithKey, targetParam);
      }

      return expression as Expression<Func<T, object>> ?? throw exception;
   }

   public IEnumerable<IGMap<T>> GetCurrentMaps()
   {
      return _mappings;
   }

   public IEnumerable<IGMap<T>> GetCurrentMapsByType(HashSet<Type> targetTypes)
   {
      foreach (var map in _mappings)
      {
         switch (map.To.Body)
         {
            case UnaryExpression unaryExpression:
            {
               if (targetTypes.Contains(unaryExpression.Operand.Type))
               {
                  yield return map;
               }

               break;
            }
            case MethodCallExpression methodCallExpression:
            {
               if (targetTypes.Contains(methodCallExpression.Type))
               {
                  yield return map;
               }

               break;
            }
            case MemberExpression memberExpression:
            {
               if (targetTypes.Contains(memberExpression.Type))
               {
                  yield return map;
               }

               break;
            }
         }
      }
   }

   public IEnumerable<IGMap<T>> GetCurrentMapsByType<TTarget>()
   {
      return GetCurrentMapsByType([typeof(TTarget)]);
   }

   /// <summary>
   /// Converts current mappings to a comma seperated list of map names.
   /// eg, field1,field2,field3
   /// </summary>
   /// <returns>a comma seperated string</returns>
   public override string ToString() => string.Join(",", _mappings.Select(q => q.From));

   internal static Expression<Func<T, object>> CreateExpression(string from)
   {
      // Param_x =>
      var parameter = Expression.Parameter(typeof(T), "__" + typeof(T).Name);
      // Param_x.Name, Param_x.yyy.zz.xx
      var mapProperty = from.Split('.').Aggregate<string, Expression>(parameter, CreatePropertyAccessExrpression);

      if (mapProperty is MethodCallExpression methodCallExpression
          && methodCallExpression.Method.Name.Equals("Select", StringComparison.InvariantCultureIgnoreCase)
          && methodCallExpression.Arguments.Last() is LambdaExpression)
      {
         return Expression.Lambda<Func<T, object>>(methodCallExpression, parameter);
      }

      if (!mapProperty.Type.IsSimpleTypeCollection(out var genericType))
      {
         // (object)Param_x.Name
         var convertedExpression = Expression.Convert(mapProperty, typeof(object));
         // Param_x => (object)Param_x.Name
         return Expression.Lambda<Func<T, object>>(convertedExpression, parameter);
      }

      var selectMethod = genericType!.GetSelectMethod();
      var predicateParameter = Expression.Parameter(genericType!);
      var predicate = Expression.Lambda(predicateParameter, predicateParameter);
      //  Param_x.Name.Select(fc => fc)
      var body = Expression.Call(selectMethod, mapProperty, predicate);
      return Expression.Lambda<Func<T, object>>(body, parameter);
   }

   internal static Expression CreatePropertyAccessExrpression(Expression expression, string propertyName)
   {
      Type? itemType;

      if (
         ((expression is MemberExpression memberExpression && memberExpression.Member is PropertyInfo propertyInfo &&
           propertyInfo.PropertyType.IsComplexTypeCollection(out itemType)) ||
          (expression is MethodCallExpression methodCallExpression && methodCallExpression.Type.IsComplexTypeCollection(out itemType)))
         && itemType is not null
      )
      {
         var selectFunction = "Select";
         var predicateParameter = Expression.Parameter(itemType);
         var propertyType =
            itemType.GetProperties().FirstOrDefault(p => p.Name.Equals(propertyName, StringComparison.InvariantCultureIgnoreCase))?.PropertyType ??
            throw new GridifyMapperException($"Property '{propertyName}' not found.");

         if (propertyType.IsComplexTypeCollection(out var propItemType) && propItemType is not null)
         {
            selectFunction = "SelectMany";
            propertyType = propItemType;
         }

         var predicate = Expression.Lambda(Expression.Property(predicateParameter, propertyName), predicateParameter);
         var selectMethod = typeof(Enumerable).GetMethods().First(m => m.Name.Equals(selectFunction, StringComparison.InvariantCulture))
            .MakeGenericMethod([itemType, propertyType]);

         var selectExpression = Expression.Call(selectMethod, expression, predicate);

         return selectExpression;
      }

      return Expression.Property(expression, propertyName);
   }

   /// <summary>
   /// Helper method to extract property name from expression and convert to camelCase
   /// </summary>
   private static string ExtractPropertyNameInCamelCase<TProperty>(Expression<Func<T, TProperty>> propertyExpression)
   {
      string propertyName;

      if (propertyExpression.Body is MemberExpression memberExpr)
      {
         propertyName = memberExpr.Member.Name;
      }
      else if (propertyExpression.Body is UnaryExpression { Operand: MemberExpression unaryMemberExpr })
      {
         propertyName = unaryMemberExpr.Member.Name;
      }
      else
      {
         throw new GridifyMapperException("Unable to extract property name from expression. Please provide a prefix.");
      }

      // Convert to camelCase
      return $"{char.ToLowerInvariant(propertyName[0])}{propertyName.Substring(1)}";
   }
}
