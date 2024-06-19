using System;
using System.Linq.Expressions;
using System.Text.RegularExpressions;

namespace Gridify;

public class GMap<T> : IGMap<T>
{
   public string From { get; set; }
   public LambdaExpression To { get; set; }
   public Func<string, object>? Convertor { get; set; }

   public GMap(string from, Expression<Func<T, object?>> to, Func<string, object>? convertor = null)
   {
      From = from;
      To = to;
      Convertor = convertor;
   }

   internal bool IsNestedCollection() => Regex.IsMatch(To.ToString(), @"\.Select\s*\(", RegexOptions.Compiled);

   public GMap(string from, Expression<Func<T, int, object?>> to, Func<string, object>? convertor = null)
   {
      From = from;
      To = to;
      Convertor = convertor;
   }

   public GMap(string from, Expression<Func<T, string, object?>> to, Func<string, object>? convertor = null)
   {
      From = from;
      To = to;
      Convertor = convertor;
   }
}
