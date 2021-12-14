using System;

namespace Gridify;

public class GridifyFilteringException : Exception
{
   public GridifyFilteringException(string message) : base(message)
   {
   }
}