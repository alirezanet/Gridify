using System;
using System.Collections.Generic;
using System.Linq;

namespace Gridify.Reflection;

public static class SimpleTypeHelper
{
   public static bool IsCollection(this Type type, out Type? itemType)
   {
      itemType = null;
      Type genericType = type;  
      if ((type.IsGenericType && type.GetGenericTypeDefinition() == typeof(IEnumerable<>)) || (type != typeof(string) && type.GetInterfaces().Any(i => {
         if (i.IsGenericType && i.GetGenericTypeDefinition() == typeof(IEnumerable<>))
         {
            genericType = i;
            return true;
         }
         return false;
      })))
      {
         var arguments = genericType.GetGenericArguments();
         if (arguments.Length == 1)
         {
            itemType = arguments[0];
            return true;
         }
      }
      return false;
   }

   public static bool IsSimpleTypeCollection(this Type type, out Type? itemType)
   {
      itemType = null;
      if (IsCollection(type, out var collectionType) && IsSimpleType(collectionType!))
      {
         itemType = collectionType;
         return true;
      }
      return false;
   }

   public static bool IsComplexTypeCollection(this Type type, out Type? itemType)
   {
      itemType = null;
      if (IsCollection(type, out var collectionType) && !IsSimpleType(collectionType!))
      {
         itemType = collectionType;
         return true;
      }
      return false;
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
