using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gridify
{
   public class GMap<T> : IGMap<T>
   {
      public string From { get; set; }
      public Expression<Func<T, object>> To { get; set; }
      public Func<string, object> Convertor { get; set; }

      public GMap(string from, Expression<Func<T, object>> to, Func<string, object> convertor = null)
      {
         From = from;
         To = to;
         Convertor = convertor;
      }
   }
}