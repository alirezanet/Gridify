using System;

namespace Gridify;

public class GridifyQueryException : Exception
{
   public GridifyQueryException(string message) : base(message)
   {
   }
}