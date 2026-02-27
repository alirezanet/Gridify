using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace Gridify;

/// <summary>
/// Represents a composite mapping that combines multiple property expressions with OR logic.
/// </summary>
/// <typeparam name="T">The type of the entity</typeparam>
public class CompositeGMap<T> : IGMap<T>
{
   public string From { get; set; }
   public LambdaExpression To { get; set; }
   public Func<string, object>? Convertor { get; set; }

   /// <summary>
   /// Collection of expressions that will be combined with OR logic
   /// </summary>
   public IReadOnlyList<Expression<Func<T, object?>>> Expressions { get; }

   public CompositeGMap(string from, params Expression<Func<T, object?>>[] expressions)
      : this(from, null, expressions)
   {
   }

   public CompositeGMap(string from, Func<string, object>? convertor, params Expression<Func<T, object?>>[] expressions)
   {
      if (expressions == null || expressions.Length == 0)
         throw new ArgumentException("At least one expression must be provided", nameof(expressions));

      From = from;
      Expressions = expressions;
      Convertor = convertor;
      // Use the first expression as the primary To expression for compatibility
      To = expressions[0];
   }

   /// <summary>
   /// Indicates if this is a composite map with multiple expressions
   /// </summary>
   public bool IsComposite => Expressions.Count > 1;
}
