using System;
using System.Collections;

namespace Gridify.Reflection;

public static class CollectionTypeHelper
{
   public static bool IsCollection(this Type type)
   {
      return typeof(IEnumerable).IsAssignableFrom(type) && type != typeof(string);
   }
}
