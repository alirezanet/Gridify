using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace Gridify.Syntax;

internal static class SimpleTypeHelper
{
   internal static bool IsSimpleTypeCollection(this Type type, out Type? itemType)
   {
      itemType = null;
      if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(List<>))
      {
         var arguments = type.GetGenericArguments();
         if (arguments.Length != 1 || !IsSimpleType(arguments[0])) return false;

         itemType = arguments[0];
         return true;
      }
      if (!type.IsArray) return false;

      var elementType = type.GetElementType();
      if (elementType == null || !IsSimpleType(elementType)) return false;
      itemType = elementType;
      return true;
   }

   internal static bool IsSimpleType(this Type type)
   {
      // Primitive types in C# include: int, float, double, char, bool, etc.
      // Also consider the known primitive types and string explicitly
      return type.IsPrimitive || type.IsValueType ||
             (type == typeof(string)) ||
             (type == typeof(decimal)) ||
             (type == typeof(DateTime)) ||
             (type == typeof(Guid));
   }

   public static MethodInfo GetSimpleTypeSelectMethod(this Type type)
   {
      return typeof(Enumerable).GetMethods().First(m => m.Name == "Select").MakeGenericMethod([type, type]);
   }
}
