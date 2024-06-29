using System;
using System.Collections.Generic;

namespace Gridify.Reflection;

public static class SimpleTypeHelper
{
   public static bool IsSimpleTypeCollection(this Type type, out Type? itemType)
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

   public static bool IsSimpleType(this Type type)
   {
      // Primitive types in C# include: int, float, double, char, bool, etc.
      // Also consider the known primitive types and string explicitly
      return type.IsPrimitive || type.IsValueType ||
             (type == typeof(string)) ||
             (type == typeof(decimal)) ||
             (type == typeof(DateTime)) ||
             (type == typeof(Guid));
   }


}
