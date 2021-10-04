using System;
using System.Linq.Expressions;

namespace Gridify
{
   public interface IGMap<T>
   {
      string From { get; set; }
      LambdaExpression To { get; set; }
      Func<string, object>? Convertor { get; set; }
      bool IsNestedCollection { get; }
   }
}