using System.Linq.Expressions;

namespace Gridify.Syntax;

internal class GridifyOperator : IGridifyOperator
{
   private readonly Expression<OperatorParameter> _handler;
   private readonly string _name;

   public GridifyOperator(string name, Expression<OperatorParameter> handler)
   {
      _name = name;
      _handler = handler;
   }

   public string GetOperator()
   {
      return _name;
   }

   public Expression<OperatorParameter> OperatorHandler()
   {
      return _handler;
   }
}
