using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using Gridify.Syntax;

namespace Gridify.Builder;

internal static class SelectExpressionBuilder<T>
{
   public static Expression<Func<T, object>> Build(string select, IGridifyMapper<T> mapper)
   {
      var paths = SelectTokenizer.Parse(select);
      if (paths.Count == 0)
         throw new GridifySelectException("Select string is empty.");

      var ignoreUnmapped = mapper.Configuration.IgnoreNotMappedFields;
      var rootParam = Expression.Parameter(typeof(T), "x");

      var resolved = new List<ResolvedPath>(paths.Count);
      foreach (var path in paths)
      {
         try
         {
            var rp = ResolvePath(path, rootParam, mapper);
            if (rp != null) resolved.Add(rp);
         }
         catch (GridifySelectException) when (ignoreUnmapped)
         {
            // skip silently
         }
      }

      if (resolved.Count == 0)
         throw new GridifySelectException("Select produced no fields.");

      var nameComparer = mapper.Configuration.CaseSensitive
         ? StringComparer.Ordinal
         : StringComparer.OrdinalIgnoreCase;
      var rootShape = BuildShape(typeof(T), resolved, rootParam, nameComparer);
      var rootType = SelectTypeFactory.GetOrCreate(rootShape);

      var memberInit = BuildMemberInitFromExpression(rootType, rootShape, rootParam);
      var body = Expression.Convert(memberInit, typeof(object));
      return Expression.Lambda<Func<T, object>>(body, rootParam);
   }

   private sealed class ResolvedPath
   {
      public IReadOnlyList<string> Segments { get; set; } = null!;
      public Expression Body { get; set; } = null!;
      public Type LeafType { get; set; } = null!;
   }

   private static ResolvedPath? ResolvePath(string path, ParameterExpression rootParam, IGridifyMapper<T> mapper)
   {
      // (a) Try full-path mapper key.
      if (mapper.HasMap(path))
      {
         var mapExp = mapper.GetExpression(path);
         var mapBody = StripConvert(mapExp.Body);
         var rebound = ReplaceParameter(mapBody, mapExp.Parameters[0], rootParam);
         return new ResolvedPath
         {
            Segments = path.Split('.'),
            Body = rebound,
            LeafType = rebound.Type
         };
      }

      // (b)/(c) Try progressively shorter prefixes; resolve remaining segments by Expression.Property walks.
      var segments = path.Split('.');
      for (var prefixLen = segments.Length - 1; prefixLen >= 1; prefixLen--)
      {
         var prefix = string.Join(".", segments.Take(prefixLen));
         if (!mapper.HasMap(prefix)) continue;

         var mapExp = mapper.GetExpression(prefix);
         var mapBody = StripConvert(mapExp.Body);
         var rebound = ReplaceParameter(mapBody, mapExp.Parameters[0], rootParam);
         var current = rebound;

         for (var i = prefixLen; i < segments.Length; i++)
            current = WalkSegment(current, segments[i]);

         return new ResolvedPath
         {
            Segments = segments,
            Body = current,
            LeafType = current.Type
         };
      }

      // (d) No mapper key matched any prefix — try resolving directly off T as a last resort.
      try
      {
         Expression current = rootParam;
         foreach (var seg in segments)
            current = WalkSegment(current, seg);
         return new ResolvedPath
         {
            Segments = segments,
            Body = current,
            LeafType = current.Type
         };
      }
      catch (GridifySelectException)
      {
         // Preserve actionable messages from WalkSegment (e.g. multi-level collection).
         throw;
      }
      catch
      {
         throw new GridifySelectException($"Field '{path}' is not mapped");
      }
   }

   private static Expression WalkSegment(Expression current, string segment)
   {
      // If current is already a collection, deeper nesting is not supported.
      if (TryGetEnumerableElementType(current.Type, out _) && current.Type != typeof(string))
      {
         throw new GridifySelectException(
            $"Path projects through more than one collection level (segment '{segment}')");
      }

      var prop = current.Type.GetProperty(segment, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
      if (prop == null)
         throw new GridifySelectException($"Path is not a property of type '{current.Type.Name}': '{segment}'");

      return Expression.Property(current, prop);
   }

   private static SelectShape BuildShape(Type sourceType, List<ResolvedPath> paths, ParameterExpression rootParam, StringComparer nameComparer)
   {
      var shape = new SelectShape { SourceType = sourceType };
      var groups = paths
         .GroupBy(p => p.Segments[0], nameComparer)
         .ToList();

      foreach (var group in groups)
      {
         var first = group.First();
         var firstSegmentExpr = ExtractFirstSegmentExpr(first.Body, rootParam);
         var firstSegmentType = firstSegmentExpr.Type;

         // Leaf: only this single segment.
         var allSingleSegment = group.All(p => p.Segments.Count == 1);
         if (allSingleSegment)
         {
            shape.Children.Add(new SelectNode
            {
               Name = group.Key,
               Accessor = Expression.Lambda(firstSegmentExpr, rootParam),
               ResultType = firstSegmentType
            });
            continue;
         }

         // Nested or collection.
         var isCollection = TryGetEnumerableElementType(firstSegmentType, out var elementType) && firstSegmentType != typeof(string);
         var childSourceType = isCollection ? elementType! : firstSegmentType;
         var childParam = Expression.Parameter(childSourceType, "y");

         // Build child paths re-anchored at childParam.
         var childPaths = group
            .Where(p => p.Segments.Count > 1)
            .Select(p => RebaseToChild(p, childParam))
            .ToList();

         var childShape = BuildShape(childSourceType, childPaths, childParam, nameComparer);
         var emittedChildType = SelectTypeFactory.GetOrCreate(childShape);

         shape.Children.Add(new SelectNode
         {
            Name = group.Key,
            Accessor = Expression.Lambda(firstSegmentExpr, rootParam),
            ResultType = emittedChildType,
            ChildShape = childShape,
            IsCollection = isCollection,
            ElementType = elementType
         });
      }

      return shape;
   }

   /// <summary>
   /// Extracts the first-segment expression from a body expression.
   /// Unwraps Select/SelectMany calls to reach the source, then walks inward
   /// through MemberExpression chains to find the property directly on <paramref name="rootParam"/>.
   /// </summary>
   private static Expression ExtractFirstSegmentExpr(Expression body, ParameterExpression rootParam)
   {
      var current = body;

      // Unwrap Select/SelectMany calls to get to the source collection expression.
      while (current is MethodCallExpression mce
             && (mce.Method.Name == "Select" || mce.Method.Name == "SelectMany")
             && mce.Arguments.Count >= 1)
      {
         current = mce.Arguments[0];
      }

      // Walk MemberExpression chain inward to find the member directly on rootParam.
      while (current is MemberExpression me)
      {
         if (me.Expression == rootParam)
            return me;
         current = me.Expression!;
      }

      return body; // fallback for unusual expressions
   }

   private static ResolvedPath RebaseToChild(ResolvedPath original, ParameterExpression childParam)
   {
      // Reconstruct the body by walking childParam.<segments[1..]>.
      Expression current = childParam;
      for (var i = 1; i < original.Segments.Count; i++)
         current = WalkSegment(current, original.Segments[i]);

      return new ResolvedPath
      {
         Segments = original.Segments.Skip(1).ToArray(),
         Body = current,
         LeafType = current.Type
      };
   }

   private static MemberInitExpression BuildMemberInitFromExpression(Type emittedType, SelectShape shape, Expression nestedSource)
   {
      var bindings = new List<MemberBinding>();
      foreach (var child in shape.Children)
      {
         var prop = emittedType.GetProperty(child.Name)
            ?? throw new InvalidOperationException($"Emitted property '{child.Name}' missing on {emittedType.Name}");

         Expression value;
         if (child.ChildShape == null)
         {
            // Leaf: substitute nestedSource in place of the accessor's parameter.
            value = ReplaceParameter(child.Accessor.Body, child.Accessor.Parameters[0], nestedSource);
         }
         else if (child.IsCollection)
         {
            // Build inner MemberInit lambda over element param.
            var elementType = child.ElementType!;
            var elementParam = Expression.Parameter(elementType, "e");
            var innerInit = BuildMemberInitFromExpression(child.ResultType, child.ChildShape, elementParam);
            var innerLambda = Expression.Lambda(innerInit, elementParam);

            var collectionExpr = ReplaceParameter(child.Accessor.Body, child.Accessor.Parameters[0], nestedSource);
            var selectMethod = typeof(Enumerable).GetMethods()
               .First(m => m.Name == nameof(Enumerable.Select)
                  && m.GetParameters().Length == 2
                  && m.GetParameters()[1].ParameterType.GetGenericArguments().Length == 2)
               .MakeGenericMethod(elementType, child.ResultType);
            value = Expression.Call(selectMethod, collectionExpr, innerLambda);
         }
         else
         {
            // Nested object: build inner MemberInit using the nested source expression.
            var deeperSource = ReplaceParameter(child.Accessor.Body, child.Accessor.Parameters[0], nestedSource);
            value = BuildMemberInitFromExpression(child.ResultType, child.ChildShape, deeperSource);
         }

         bindings.Add(Expression.Bind(prop, value));
      }
      return Expression.MemberInit(Expression.New(emittedType), bindings);
   }

   private static Expression StripConvert(Expression e)
   {
      while (e is UnaryExpression u && (u.NodeType == ExpressionType.Convert || u.NodeType == ExpressionType.ConvertChecked))
         e = u.Operand;
      return e;
   }

   private static Expression ReplaceParameter(Expression body, ParameterExpression oldParam, Expression newExpr)
   {
      return new ParameterReplacer(oldParam, newExpr).Visit(body)!;
   }

   private sealed class ParameterReplacer(ParameterExpression oldParam, Expression newExpr) : ExpressionVisitor
   {
      protected override Expression VisitParameter(ParameterExpression node) =>
         node == oldParam ? newExpr : base.VisitParameter(node);
   }

   private static bool TryGetEnumerableElementType(Type type, out Type? elementType)
   {
      if (type.IsArray)
      {
         elementType = type.GetElementType();
         return true;
      }
      foreach (var i in type.GetInterfaces().Concat(new[] { type }))
      {
         if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
         {
            elementType = i.GetGenericArguments()[0];
            return true;
         }
      }
      elementType = null;
      return false;
   }
}
