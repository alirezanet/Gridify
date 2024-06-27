using System;
using System.Linq;
using System.Linq.Expressions;
using Gridify.Syntax;

namespace Gridify.QueryBuilders;

internal class LinqSortingQueryBuilder<T>(IGridifyMapper<T>? mapper = null) : BaseSortingQueryBuilder<IQueryable<T>, T>(mapper)
{
   protected override IQueryable<T> ApplySorting(IQueryable<T> query, ParsedOrdering ordering)
   {
      return query.OrderByMember(GetOrderExpression(ordering), ordering.IsAscending);
   }

   protected override IQueryable<T> ApplyAnotherSorting(IQueryable<T> query, ParsedOrdering ordering)
   {
      return query.ThenByMember(GetOrderExpression(ordering), ordering.IsAscending);
   }

   private Expression<Func<T, object>> GetOrderExpression(ParsedOrdering ordering)
   {
      var exp = mapper!.GetExpression(ordering.MemberName);
      switch (ordering.OrderingType)
      {
         case OrderingType.Normal:
            return exp;
         case OrderingType.NullCheck:
         case OrderingType.NotNullCheck:
         default:
         {
            // member should be nullable
            if (exp.Body is not UnaryExpression unary || Nullable.GetUnderlyingType(unary.Operand.Type) == null)
            {
               throw new GridifyOrderingException($"'{ordering.MemberName}' is not nullable type");
            }

            var prop = Expression.Property(exp.Parameters[0], ordering.MemberName);
            var hasValue = Expression.PropertyOrField(prop, "HasValue");

            switch (ordering.OrderingType)
            {
               case OrderingType.NullCheck:
               {
                  var boxedExpression = Expression.Convert(hasValue, typeof(object));
                  return Expression.Lambda<Func<T, object>>(boxedExpression, exp.Parameters);
               }
               case OrderingType.NotNullCheck:
               {
                  var notHasValue = Expression.Not(hasValue);
                  var boxedExpression = Expression.Convert(notHasValue, typeof(object));
                  return Expression.Lambda<Func<T, object>>(boxedExpression, exp.Parameters);
               }
               // should never reach here
               case OrderingType.Normal:
                  return exp;
               default:
                  throw new ArgumentOutOfRangeException();
            }
         }
      }
   }
}
