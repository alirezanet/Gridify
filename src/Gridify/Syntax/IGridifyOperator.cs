using System;
using System.Linq.Expressions;

namespace Gridify.Syntax;

/// <summary>
/// By implementing this interface, you can define your own custom operators.
/// Custom operators must start with the '#' character
/// </summary>
public interface IGridifyOperator
{
   /// <summary>
   /// The Custom operator name
   /// Custom operators must start with the '#' character
   /// E.g: #=
   /// </summary>
   /// <returns>Custom operator name</returns>
   string GetOperator();

   /// <summary>
   /// Using OperationHandler, you can define your own custom operator logic
   /// </summary>
   /// <returns></returns>
   Expression<OperatorParameter> OperatorHandler();
}

public delegate bool OperatorParameter(object propertyReference, object value);
