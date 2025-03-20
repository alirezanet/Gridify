using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gridify.Syntax;

public class OperatorManager
{
   private readonly ConcurrentDictionary<string, IGridifyOperator> _operators = new();
   public IEnumerable<IGridifyOperator> Operators => _operators?.Values ?? Enumerable.Empty<IGridifyOperator>();

   public void Register<TOperator>() where TOperator : IGridifyOperator
   {
      var gridifyOperator = (IGridifyOperator)Activator.CreateInstance(typeof(TOperator))!;
      Register(gridifyOperator);
   }

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
   public void Remove<TOperator>() where TOperator : IGridifyOperator
   {
      var gridifyOperator = (IGridifyOperator)Activator.CreateInstance(typeof(TOperator))!;
      Remove(gridifyOperator.GetOperator());
   }

   public void Remove(string @operator) => _operators?.Remove(@operator);

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
      var @operator = gridifyOperator.GetOperator();
      operators.TryAdd(@operator, gridifyOperator);
   }

   internal static void Remove(this ConcurrentDictionary<string, IGridifyOperator> operators, string @operator)
   {
      operators.TryRemove(@operator, out _);
   }
}
