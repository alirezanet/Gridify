using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gridify
{
   public interface IGridifyMapper<T>
   {
      Dictionary<string, Expression<Func<T, object>>> Mappings { get; set; }
      bool CaseSensitive { get; }

      GridifyMapper<T> AddMap (string propertyName, Expression<Func<T, object>> column, bool replaceOldMapping = true);
      GridifyMapper<T> GenerateMappings ();
      GridifyMapper<T> RemoveMap (string propertyName);
   }
}