using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace Gridify.Syntax;

public class OperatorManager
{
   internal readonly List<IGridifyOperator> Operators = new();

   public void Register(IGridifyOperator gridifyOperator)
   {
      Validate(gridifyOperator.GetOperator());

      Operators.Add(gridifyOperator);
   }

   public void Register(string name, Expression<OperatorParameter> handler)
   {
      Validate(name);

      var customOperator = new GridifyOperator(name, handler);
      Operators.Add(customOperator);
   }

   private static void Validate(string operatorName)
   {
      if (!operatorName.StartsWith("#"))
         throw new ArgumentException("Custom operator must start with '#'");
   }
}
