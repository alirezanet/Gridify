using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gridify.Syntax;

public class OperatorManager
{
   private readonly ConcurrentDictionary<string, IGridifyOperator> _operators = new();
   public IEnumerable<IGridifyOperator> Operators => _operators.Values;

   public void Register<TOperator>() where TOperator : IGridifyOperator
   {
      var gridifyOperator = (IGridifyOperator)Activator.CreateInstance(typeof(TOperator))!;
      Register(gridifyOperator);
   }

   public void Register(IGridifyOperator gridifyOperator)
   {
      Validate(gridifyOperator.GetOperator());
      var @operator = gridifyOperator.GetOperator();
      _operators.TryAdd(@operator, gridifyOperator);
   }

   public void Register(string @operator, Expression<OperatorParameter> handler)
   {
      Validate(@operator);

      var customOperator = new GridifyOperator(@operator, handler);
      _operators.TryAdd(@operator, customOperator);
   }
   public void Remove<TOperator>() where TOperator : IGridifyOperator
   {
      var gridifyOperator = (IGridifyOperator)Activator.CreateInstance(typeof(TOperator))!;
      Remove(gridifyOperator.GetOperator());
   }

   public void Remove(string @operator) => _operators.TryRemove(@operator, out _);

   private static void Validate(string @operator)
   {
      if (!@operator.StartsWith("#"))
         throw new ArgumentException("Custom operator must start with '#'");
   }
}
