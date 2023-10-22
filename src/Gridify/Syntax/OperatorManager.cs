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

      _operators.TryAdd(gridifyOperator.GetOperator(), gridifyOperator);
   }

   public void Register(string name, Expression<OperatorParameter> handler)
   {
      Validate(name);

      var customOperator = new GridifyOperator(name, handler);
      _operators.TryAdd(customOperator.GetOperator(), customOperator);
   }

   public bool Remove(string operatorSymbol)
      => _operators.TryRemove(operatorSymbol, out _);

   private static void Validate(string operatorName)
   {
      if (!operatorName.StartsWith("#"))
         throw new ArgumentException("Custom operator must start with '#'");
   }
}
