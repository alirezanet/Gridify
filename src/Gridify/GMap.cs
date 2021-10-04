using System;
using System.Linq.Expressions;

namespace Gridify
{
   public class GMap<T> : IGMap<T>
   {
      public string From { get; set; }
      public LambdaExpression To { get; set; }
      public Func<string, object>? Convertor { get; set; }
      public bool IsNestedCollection { get; }

      public GMap(string from, Expression<Func<T, object?>> to, Func<string, object>? convertor = null, bool isNestedCollection = false)
      {
         From = from;
         To = to;
         Convertor = convertor;
         IsNestedCollection = isNestedCollection;
      }
      
      public GMap(string from, Expression<Func<T,int, object?>> to, Func<string, object>? convertor = null, bool isNestedCollection = false)
      {
         From = from;
         To = to;
         Convertor = convertor;
         IsNestedCollection = isNestedCollection;
      }
   }
}