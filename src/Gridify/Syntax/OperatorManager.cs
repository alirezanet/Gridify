using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gridify.Syntax;

public class OperatorManager
{
   private readonly ConcurrentDictionary<string, IGridifyOperator> _operators = new();
   internal IEnumerable<IGridifyOperator> Operators => _operators.Values;

   public void Register(IGridifyOperator gridifyOperator)
   {
      Validate(gridifyOperator.GetOperator());

      _operators.Add(gridifyOperator);
   }

   public void Register(string @operator, Expression<OperatorParameter> handler)
   {
      Validate(@operator);

      var customOperator = new GridifyOperator(@operator, handler);
      _operators.Add(customOperator);
   }

   public void Remove(string @operator) => _operators.Remove(@operator);

   private static void Validate(string @operator)
   {
      if (!@operator.StartsWith("#"))
         throw new ArgumentException("Custom operator must start with '#'");
   }
}

/// <summary>
/// Retry if for some reason we couldn't add/remove the operator
/// </summary>
internal static class OperatorManagerExtensions
{
   internal static void Add(this ConcurrentDictionary<string, IGridifyOperator> operators, IGridifyOperator gridifyOperator)
   {
      var retry = 0;
      while (true)
      {
         if (operators.TryAdd(gridifyOperator.GetOperator(), gridifyOperator))
         {
            return;
         }

         if (retry >= 3)
         {
            throw new Exception("Unexpected error! Can not add GridifyOperator");
         }
         retry++;
      }
   }

   internal static void Remove(this ConcurrentDictionary<string, IGridifyOperator> operators, string @operator)
   {
      var retry = 0;
      while (true)
      {
         if (operators.TryRemove(@operator, out _))
         {
            return;
         }

         if (retry >= 3)
         {
            throw new Exception("Unexpected error! Can not remove GridifyOperator");
         }
         retry++;
      }
   }
}
