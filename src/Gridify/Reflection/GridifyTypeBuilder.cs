using System;
using System.Collections.Concurrent;
using System.Reflection;
using System.Reflection.Emit;

namespace Gridify.Reflection;

public static class GridifyTypeBuilder
{
   private static readonly ConcurrentDictionary<Type, TypeInfo> Cache = new();

   public static (object? myObject, Type myType) CreateNewObject(Type type, string fieldName, object? value)
   {
      TypeInfo? myTypeInfo;
      if (Cache.TryGetValue(type, out var expectedTypeInfo))
         myTypeInfo = expectedTypeInfo;
      else
      {
         myTypeInfo = CompileResultTypeInfo(type, fieldName);
         Cache.TryAdd(type, myTypeInfo!);
      }

      var myType = myTypeInfo!.AsType();
      var myObject = Activator.CreateInstance(myType);
      myType.GetProperty(fieldName)!.SetValue(myObject, value);
      return (myObject, myType);
   }

   public static TypeInfo? CompileResultTypeInfo(Type type, string name)
   {
      var tb = GetTypeBuilder();
      var constructor =
         tb.DefineDefaultConstructor(MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName);

      var field = new FieldDescriptor(name, type);
      CreateProperty(tb, field.FieldName, field.FieldType);
      var objectTypeInfo = tb.CreateTypeInfo();

      return objectTypeInfo;
   }

   private static TypeBuilder GetTypeBuilder()
   {
      const string? typeSignature = "GridifyDisplayClass";
      var an = new AssemblyName(typeSignature);
      var assemblyBuilder = AssemblyBuilder.DefineDynamicAssembly(new AssemblyName(Guid.NewGuid().ToString()), AssemblyBuilderAccess.Run);
      var moduleBuilder = assemblyBuilder.DefineDynamicModule("RuntimeModule");
      var tb = moduleBuilder.DefineType(typeSignature,
         TypeAttributes.Public |
         TypeAttributes.Class |
         TypeAttributes.AutoClass |
         TypeAttributes.AnsiClass |
         TypeAttributes.BeforeFieldInit |
         TypeAttributes.AutoLayout,
         null);
      return tb;
   }

   private static void CreateProperty(TypeBuilder tb, string propertyName, Type propertyType)
   {
      var fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

      var propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
      var getPropMthdBldr = tb.DefineMethod("get_" + propertyName,
         MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
      var getIl = getPropMthdBldr.GetILGenerator();

      getIl.Emit(OpCodes.Ldarg_0);
      getIl.Emit(OpCodes.Ldfld, fieldBuilder);
      getIl.Emit(OpCodes.Ret);

      var setPropMthdBldr =
         tb.DefineMethod("set_" + propertyName,
            MethodAttributes.Public |
            MethodAttributes.SpecialName |
            MethodAttributes.HideBySig,
            null, [propertyType]);

      var setIl = setPropMthdBldr.GetILGenerator();
      var modifyProperty = setIl.DefineLabel();
      var exitSet = setIl.DefineLabel();

      setIl.MarkLabel(modifyProperty);
      setIl.Emit(OpCodes.Ldarg_0);
      setIl.Emit(OpCodes.Ldarg_1);
      setIl.Emit(OpCodes.Stfld, fieldBuilder);

      setIl.Emit(OpCodes.Nop);
      setIl.MarkLabel(exitSet);
      setIl.Emit(OpCodes.Ret);

      propertyBuilder.SetGetMethod(getPropMthdBldr);
      propertyBuilder.SetSetMethod(setPropMthdBldr);
   }

   private class FieldDescriptor(string fieldName, Type fieldType)
   {
      public string FieldName { get; } = fieldName;
      public Type FieldType { get; } = fieldType;
   }
}
