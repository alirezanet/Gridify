using System.Collections.Generic;
using System.Linq;

namespace Gridify;

public class Paging<T>(int count, IEnumerable<T> data)
{
   public Paging() : this(0, Enumerable.Empty<T>())
   {
   }
   public void Deconstruct(out int count, out IEnumerable<T> data)
   {
      count = Count;
      data = Data;
   }

   public int Count { get; set; } = count;
   public IEnumerable<T> Data { get; set; } = data;
}
