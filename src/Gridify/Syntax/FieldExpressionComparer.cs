using System;
using System.Collections.Generic;

namespace Gridify.Syntax;

public class FieldExpressionComparer : IEqualityComparer<FieldExpressionSyntax>
{
   public bool Equals(FieldExpressionSyntax? x, FieldExpressionSyntax? y)
   {
      if (ReferenceEquals(x, null)) return false;
      if (ReferenceEquals(y, null)) return false;
      if (ReferenceEquals(x, y)) return true;
      if (x.GetType() != y.GetType()) return false;
      return x.Indexer == y.Indexer && x.FieldToken.Text.Equals(y.FieldToken.Text);
   }

   public int GetHashCode(FieldExpressionSyntax obj)
   {
#if NETSTANDARD2_0
    unchecked
    {
        var hash = 17;
        if (obj.Indexer != null)
         hash = hash * 23 + obj.Indexer.GetHashCode();
        hash = hash * 23 + obj.FieldToken.Text.GetHashCode();
        return hash;
    }
#else
      return HashCode.Combine(obj.Indexer, obj.FieldToken.Text);
#endif
   }
}
