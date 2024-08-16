using System;
using System.Linq;
using System.Reflection;

namespace Gridify.Reflection;

public static class MethodInfoHelper
{
   public static MethodInfo GetAnyMethod(Type type)
   {
      return typeof(Enumerable).GetMethods().Single(m => m.Name == "Any" && m.GetParameters().Length == 2).MakeGenericMethod(type);
   }

   public static MethodInfo GetEndsWithMethod()
   {
      return typeof(string).GetMethod("EndsWith", [typeof(string)])!;
   }

   public static MethodInfo GetStartWithMethod()
   {
      return typeof(string).GetMethod("StartsWith", [typeof(string)])!;
   }

   public static MethodInfo GetStringContainsMethod()
   {
      return typeof(string).GetMethod("Contains", [typeof(string)])!;
   }

   public static MethodInfo GetContainsMethod(Type tp)
   {
      return typeof(Enumerable).GetMethods().First(x => x.Name == "Contains").MakeGenericMethod(tp);
   }

   public static MethodInfo GetIsNullOrEmptyMethod()
   {
      return typeof(string).GetMethod("IsNullOrEmpty", [typeof(string)])!;
   }

   public static MethodInfo GetCompareMethodWithStringComparison()
   {
      return typeof(string).GetMethod("Compare", [typeof(string), typeof(string), typeof(StringComparison)])!;
   }

   public static MethodInfo GetCompareMethod()
   {
      return typeof(string).GetMethod("Compare", [typeof(string), typeof(string)])!;
   }

   public static MethodInfo GetToStringMethod()
   {
      return typeof(object).GetMethod("ToString")!;
   }

   public static MethodInfo GetSelectMethod(this Type type)
   {
      return typeof(Enumerable).GetMethods().First(m => m.Name == "Select").MakeGenericMethod([type, type]);
   }

   public static MethodInfo GetCaseAwareContainsMethod(Type tp)
   {
      return typeof(Enumerable).GetMethods().Last(x => x.Name == "Contains").MakeGenericMethod(tp);
   }

   public static MethodInfo GetCaseAwareStringContainsMethod()
   {
      return typeof(string).GetMethod("Contains", [typeof(string), typeof(StringComparison)])!;
   }

   public static MethodInfo GetCaseAwareEqualsMethod()
   {
      return typeof(string).GetMethod("Equals", [typeof(string), typeof(string), typeof(StringComparison)])!;
   }

   public static MethodInfo GetCaseAwareStartsWithMethod()
   {
      return typeof(string).GetMethod("StartsWith", [typeof(string), typeof(StringComparison)])!;
   }

   public static MethodInfo GetCaseAwareEndsWithMethod()
   {
      return typeof(string).GetMethod("EndsWith", [typeof(string), typeof(StringComparison)])!;
   }
}
