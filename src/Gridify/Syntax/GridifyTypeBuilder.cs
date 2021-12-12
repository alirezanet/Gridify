using System;
using System.Reflection;
using System.Reflection.Emit;

namespace Gridify.Syntax;

internal static class GridifyTypeBuilder
{
   private class FieldDescriptor
   {
      public FieldDescriptor(string fieldName, Type fieldType)
      {
         FieldName = fieldName;
         FieldType = fieldType;
      }

      public string FieldName { get; }
      public Type FieldType { get; }
   }

   public static (object Instance, Type type) CreateNewObject(Type type, string fieldName, object? value)
   {
      var myTypeInfo = CompileResultTypeInfo(type, fieldName);
      var myType = myTypeInfo!.AsType();
      var myObject = Activator.CreateInstance(myType);
      myType.GetProperty(fieldName)!.SetValue(myObject, value);
      return (myObject, myType);
   }

   public static TypeInfo? CompileResultTypeInfo(Type type, string name)
   {
      TypeBuilder tb = GetTypeBuilder();
      ConstructorBuilder constructor =
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
      ModuleBuilder moduleBuilder = assemblyBuilder.DefineDynamicModule("RuntimeModule");
      TypeBuilder tb = moduleBuilder.DefineType(typeSignature,
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
      FieldBuilder fieldBuilder = tb.DefineField("_" + propertyName, propertyType, FieldAttributes.Private);

      PropertyBuilder propertyBuilder = tb.DefineProperty(propertyName, PropertyAttributes.HasDefault, propertyType, null);
      MethodBuilder getPropMthdBldr = tb.DefineMethod("get_" + propertyName,
         MethodAttributes.Public | MethodAttributes.SpecialName | MethodAttributes.HideBySig, propertyType, Type.EmptyTypes);
      ILGenerator getIl = getPropMthdBldr.GetILGenerator();

      getIl.Emit(OpCodes.Ldarg_0);
      getIl.Emit(OpCodes.Ldfld, fieldBuilder);
      getIl.Emit(OpCodes.Ret);

      MethodBuilder setPropMthdBldr =
         tb.DefineMethod("set_" + propertyName,
            MethodAttributes.Public |
            MethodAttributes.SpecialName |
            MethodAttributes.HideBySig,
            null, new[] { propertyType });

      ILGenerator setIl = setPropMthdBldr.GetILGenerator();
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
}