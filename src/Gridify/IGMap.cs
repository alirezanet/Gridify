using System;
using System.Linq.Expressions;

namespace Gridify;

public interface IGMap<T>
{
   string From { get; set; }
   LambdaExpression To { get; set; }
   Func<string, object>? Convertor { get; set; }
   // null = use global config, true = force case-insensitive, false = force case-sensitive
   bool? CaseInsensitive { get; set; }
}
