using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Reflection.Emit;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading;

namespace Gridify.Builder;

/// <summary>
/// Emits and caches runtime CLR types matching a <see cref="SelectShape"/> so each unique
/// projection shape is materialized as exactly one type per process.
/// </summary>
/// <remarks>
/// The cache is process-wide and unbounded — emitted types live for the lifetime of the
/// dynamic assembly. For typical use (a finite set of select strings per endpoint) the cache
/// is naturally bounded. If select strings can come from untrusted input and produce
/// arbitrarily many distinct shapes, validate / restrict the input before calling
/// <see cref="GridifyExtensions.ApplySelect{T}(System.Linq.IQueryable{T}, string?, IGridifyMapper{T}?)"/>
/// (e.g. via <see cref="GridifyExtensions.IsValidSelect{T}(IGridifySelecting, IGridifyMapper{T})"/>)
/// to keep cache growth predictable.
/// </remarks>
internal static class SelectTypeFactory
{
   private static readonly ConcurrentDictionary<string, Type> Cache = new();
   private static readonly object EmissionLock = new();

   private static ModuleBuilder? _module;

   private static ModuleBuilder Module
   {
      get
      {
         if (_module != null) return _module;
         lock (EmissionLock)
         {
            if (_module != null) return _module;
            var asmName = new AssemblyName("Gridify.Dynamic");
            var asm = AssemblyBuilder.DefineDynamicAssembly(asmName, AssemblyBuilderAccess.Run);
            _module = asm.DefineDynamicModule("Gridify.Dynamic");
            return _module;
         }
      }
   }

   public static Type GetOrCreate(SelectShape shape)
   {
#if NET8_0_OR_GREATER
      if (!RuntimeFeature.IsDynamicCodeSupported)
         throw new GridifySelectException(
            "Select requires JIT support; not available under NativeAOT. Track issue #140.");
#endif

      var signature = BuildSignature(shape);

      if (Cache.TryGetValue(signature, out var existing))
         return existing;

      lock (EmissionLock)
      {
         if (Cache.TryGetValue(signature, out existing))
            return existing;

         var emitted = EmitType(shape, signature);
         Cache[signature] = emitted;
         return emitted;
      }
   }

   private static string BuildSignature(SelectShape shape)
   {
      var sb = new StringBuilder();
      sb.Append(shape.SourceType.FullName ?? shape.SourceType.Name);
      sb.Append('|');
      foreach (var child in shape.Children.OrderBy(c => c.Name, StringComparer.Ordinal))
      {
         sb.Append(child.Name);
         sb.Append(':');
         sb.Append(child.ResultType.FullName ?? child.ResultType.Name);
         if (child.IsCollection) sb.Append("[]");
         if (child.ChildShape != null)
         {
            sb.Append('{');
            sb.Append(BuildSignature(child.ChildShape));
            sb.Append('}');
         }
         sb.Append(';');
      }
      return sb.ToString();
   }

   private static int _nameCounter;

   private static Type EmitType(SelectShape shape, string signature)
   {
      var hash = ((uint)signature.GetHashCode()).ToString("X");
      var counter = Interlocked.Increment(ref _nameCounter);
      var typeName = "Gridify_Select_" + hash + "_" + counter.ToString("X");

      var tb = Module.DefineType(
         typeName,
         TypeAttributes.Public | TypeAttributes.Class | TypeAttributes.Sealed |
         TypeAttributes.AutoClass | TypeAttributes.AnsiClass | TypeAttributes.BeforeFieldInit);

      var compilerGeneratedCtor = typeof(CompilerGeneratedAttribute).GetConstructor(Type.EmptyTypes)!;
      tb.SetCustomAttribute(new CustomAttributeBuilder(compilerGeneratedCtor, Array.Empty<object>()));

      foreach (var child in shape.Children)
      {
         var propType = child.IsCollection
            ? typeof(IEnumerable<>).MakeGenericType(child.ResultType)
            : child.ResultType;
         AddAutoProperty(tb, child.Name, propType);
      }

      // Parameterless constructor — required for Expression.MemberInit + EF Core's translation.
      tb.DefineDefaultConstructor(MethodAttributes.Public);

#if NETSTANDARD2_0
      return tb.CreateTypeInfo()!.AsType();
#else
      return tb.CreateType()!;
#endif
   }

   private static void AddAutoProperty(TypeBuilder tb, string name, Type propType)
   {
      var field = tb.DefineField("_" + name, propType, FieldAttributes.Private);
      var prop = tb.DefineProperty(name, PropertyAttributes.HasDefault, propType, null);

      var getter = tb.DefineMethod(
         "get_" + name,
         MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
         propType, Type.EmptyTypes);
      var getIl = getter.GetILGenerator();
      getIl.Emit(OpCodes.Ldarg_0);
      getIl.Emit(OpCodes.Ldfld, field);
      getIl.Emit(OpCodes.Ret);

      var setter = tb.DefineMethod(
         "set_" + name,
         MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig,
         null, new[] { propType });
      var setIl = setter.GetILGenerator();
      setIl.Emit(OpCodes.Ldarg_0);
      setIl.Emit(OpCodes.Ldarg_1);
      setIl.Emit(OpCodes.Stfld, field);
      setIl.Emit(OpCodes.Ret);

      prop.SetGetMethod(getter);
      prop.SetSetMethod(setter);
   }
}
