using System;
using System.Linq.Expressions;
using Gridify.Syntax;

namespace Gridify
{
   public static partial class PredicateBuilder
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

      public static LambdaExpression And (this LambdaExpression expr1, LambdaExpression expr2)
      {
         var parameter = Expression.Parameter(expr1.Parameters[0].Type);

         var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
         var left = leftVisitor.Visit(expr1.Body);

         var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
         var right = rightVisitor.Visit(expr2.Body);

         return Expression.Lambda(Expression.AndAlso(left!, right!), parameter);
      }

      public static LambdaExpression Or  (this LambdaExpression expr1, LambdaExpression expr2)
      {
         var parameter = Expression.Parameter(expr1.Parameters[0].Type);

         var leftVisitor = new ReplaceExpressionVisitor(expr1.Parameters[0], parameter);
         var left = leftVisitor.Visit(expr1.Body);

         var rightVisitor = new ReplaceExpressionVisitor(expr2.Parameters[0], parameter);
         var right = rightVisitor.Visit(expr2.Body);

         return Expression.Lambda(Expression.OrElse(left!, right!), parameter);
      }
   }
}
