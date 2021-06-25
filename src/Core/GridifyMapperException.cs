using System;

namespace Gridify
{
   public partial class GridifyMapper<T>
   {
      public class GridifyMapperException : Exception
      {
         public GridifyMapperException(string message) : base(message)
         {
         }
      }
   }
}