using System.Linq.Expressions;

namespace Gridify.Syntax;

public class GridifyOperator(string name, Expression<OperatorParameter> handler) : IGridifyOperator
{
   public string GetOperator()
   {
      return name;
   }

   public Expression<OperatorParameter> OperatorHandler()
   {
      return handler;
   }
}
