using System;
using System.Linq.Expressions;
namespace Gridify
{
   public static class PredicateBuilder
   {
      public static Expression<Func<T, bool>> Or<T> (this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
      {
         var parameter = Expression.Parameter(typeof (T));

         var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
         var left = leftVisitor.Visit(expr1.Body);

         var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
         var right = rightVisitor.Visit(expr2.Body);

         return Expression.Lambda<Func<T, bool>>(Expression.OrElse(left!, right!), parameter);
      }

      public static Expression<Func<T, bool>> And<T> (this Expression<Func<T, bool>> expr1, Expression<Func<T, bool>> expr2)
      {
         var parameter = Expression.Parameter(typeof (T));

         var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
         var left = leftVisitor.Visit(expr1.Body);

         var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
         var right = rightVisitor.Visit(expr2.Body);

         return Expression.Lambda<Func<T, bool>>(Expression.AndAlso(left!, right!), parameter);
      }
      
      
      internal class ReplaceExpressionVisitor : ExpressionVisitor
      {
         private readonly Expression _oldValue;
         private readonly Expression _newValue;

         public ReplaceExpressionVisitor(Expression oldValue, Expression newValue)
         {
            _oldValue = oldValue;
            _newValue = newValue;
         }

         public override Expression Visit(Expression node)
         {
            return node == _oldValue ? _newValue : base.Visit(node)!;
         }
      } 
      
      public static Expression<Func<T, bool>> True<T> () { return f => true; }
      public static Expression<Func<T, bool>> False<T> () { return f => false; }

   }
}
