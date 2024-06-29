using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gridify.Syntax;

public class OperatorManager
{
   private ConcurrentDictionary<string, IGridifyOperator>? _operators;
   public IEnumerable<IGridifyOperator> Operators => _operators?.Values ?? Enumerable.Empty<IGridifyOperator>();

   public void Register<TOperator>() where TOperator : IGridifyOperator
   {
      var gridifyOperator = (IGridifyOperator)Activator.CreateInstance(typeof(TOperator))!;
      Register(gridifyOperator);
   }

   public void Register(IGridifyOperator gridifyOperator)
   {
      Validate(gridifyOperator.GetOperator());
      _operators ??= new();
      _operators.Add(gridifyOperator);
   }

   public void Register(string @operator, Expression<OperatorParameter> handler)
   {
      Validate(@operator);

      var customOperator = new GridifyOperator(@operator, handler);
      _operators ??= new();
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
      if (operators.ContainsKey(@operator)) // already exists
      {
         return;
      }

      var retry = 0;
      while (true)
      {
         if (operators.TryAdd(@operator, gridifyOperator))
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
      if (!operators.ContainsKey(@operator))
      {
         return;
      }

      var retry = 0;
      while (true)
      {
         if (operators.TryRemove(@operator, out _))
         {
            return;
         }

         if (retry >= 3)
         {
            throw new Exception("Can not remove GridifyOperator or it is not registered");
         }

         retry++;
      }
   }
}
