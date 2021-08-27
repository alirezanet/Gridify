using System;
using System.Linq.Expressions;

namespace Gridify
{
   public interface IGMap<T>
   {
       string From { get; set; }
       Expression<Func<T, object?>> To { get; set; }
       Func<string, object>? Convertor { get; set; }
   }
}