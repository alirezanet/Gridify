using System;
using System.Data;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.InteropServices;
using Gridify.Syntax;

namespace Gridify.QueryBuilders;

internal class LinqQueryBuilder<T> : BaseQueryBuilder<Expression<Func<T, bool>>, T>
{
   public LinqQueryBuilder(IGridifyMapper<T> mapper) : base(mapper)
   {
   }

   protected override Expression<Func<T, bool>>? BuildNestedQuery(
      Expression body, IGMap<T> gMap, ValueExpressionSyntax value, SyntaxNode op)
   {
      while (true)
      {
         switch (body)
         {
            case MethodCallExpression { Method.Name: "Select" } selectExp:
            {
               var targetExp = selectExp.Arguments.Single(a => a.NodeType == ExpressionType.Lambda) as LambdaExpression;
               var conditionExp = BuildQuery(
                  targetExp!.Body,
                  targetExp.Parameters[0],
                  value,
                  op,
                  gMap.Convertor);

               if (conditionExp is not LambdaExpression lambdaExp) return null;

               return ParseMethodCallExpression(selectExp, lambdaExp) as Expression<Func<T, bool>>;
            }
            case ConditionalExpression cExp:
            {
               var ifTrue = GenerateNestedExpression(cExp.IfTrue, gMap, value, op);
               ifTrue = new ReplaceExpressionVisitor(ifTrue!.Parameters[0], gMap.To.Parameters[0]).Visit(ifTrue) as Expression<Func<T, bool>>;
               var ifFalse = GenerateNestedExpression(cExp.IfFalse, gMap, value, op);
               ifFalse = new ReplaceExpressionVisitor(ifFalse!.Parameters[0], gMap.To.Parameters[0]).Visit(ifFalse) as Expression<Func<T, bool>>;

               var newExp = Expression.Condition(cExp.Test, ifTrue!.Body, ifFalse!.Body);
               return Expression.Lambda<Func<T, bool>>(newExp, gMap.To.Parameters[0]);
            }
            case ConstantExpression constantExpression:
            {
               return Expression.Lambda<Func<T, bool>>(constantExpression, gMap.To.Parameters[0]);
            }
            case UnaryExpression uExp:
            {
               body = uExp.Operand;
               continue;
            }
            default:
               // this should never happening
               throw new GridifyFilteringException($"The 'Select' method on '{gMap.From}' for type {body.Type} not found");
         }
      }
   }

   protected override Expression<Func<T, bool>> BuildAlwaysTrueQuery()
   {
      return _ => true;
   }

   protected override Expression<Func<T, bool>> BuildAlwaysFalseQuery(ParameterExpression parameter)
   {
      var expression = Expression.Lambda(Expression.Constant(false), parameter) as Expression<Func<T, bool>>;
      return expression!;
   }

   protected override Expression<Func<T, bool>>? CheckIfCanMergeQueries(
      (Expression<Func<T, bool>> query, bool isNested) leftQuery,
      (Expression<Func<T, bool>> query, bool isNested) rightQuery,
      SyntaxKind op)
   {
      if (leftQuery.isNested && rightQuery.isNested)
      {
         var leftExp = ParseNestedExpression(leftQuery.query.Body);
         var rightExp = ParseNestedExpression(rightQuery.query.Body);

         if (leftExp.Arguments.First() is MemberExpression leftMember &&
             rightExp.Arguments.First() is MemberExpression rightMember &&
             leftMember.Type == rightMember.Type)
         {
            if (leftExp.Arguments.Last() is not LambdaExpression leftLambda
                || rightExp.Arguments.Last() is not LambdaExpression rightLambda)
            {
               return null;
            }

            var visitedRight = new ReplaceExpressionVisitor(rightLambda.Parameters[0], leftLambda.Parameters[0])
               .Visit(rightLambda.Body);

            var mergedExpression = op switch
            {
               SyntaxKind.And => Expression.AndAlso(leftLambda.Body, visitedRight),
               SyntaxKind.Or => Expression.OrElse(leftLambda.Body, visitedRight),
               _ => throw new InvalidOperationException()
            };

            var mergedLambda = Expression.Lambda(mergedExpression, leftLambda.Parameters);
            return GetAnyExpression(leftMember, mergedLambda) as Expression<Func<T, bool>>;
         }
      }

      return null;
   }

   protected override object? BuildQueryAccordingToValueType(
      Expression body,
      ParameterExpression parameter,
      object? value,
      SyntaxNode op,
      ValueExpressionSyntax valueExpression)
   {
      Expression be;

      // use string.Compare instead of operators if value and field are both strings
      var areBothStrings = body.Type == typeof(string) && value?.GetType() == typeof(string);

      switch (op.Kind)
      {
         case SyntaxKind.Equal when !valueExpression.IsNullOrDefault:
            be = Expression.Equal(body, GetValueExpression(body.Type, value));
            break;
         case SyntaxKind.Equal when valueExpression.IsNullOrDefault:
            if (body.Type == typeof(string))
               be = Expression.Call(null, GetIsNullOrEmptyMethod(), body);
            else
            {
               var canBeNull = !body.Type.IsValueType || (Nullable.GetUnderlyingType(body.Type) != null);
               be = canBeNull
                  ? Expression.OrElse(Expression.Equal(body, Expression.Constant(null)), Expression.Equal(body, Expression.Default(body.Type)))
                  : Expression.Equal(body, Expression.Default(body.Type));
            }

            break;
         case SyntaxKind.NotEqual when !valueExpression.IsNullOrDefault:
            be = Expression.NotEqual(body, GetValueExpression(body.Type, value));
            break;
         case SyntaxKind.NotEqual when valueExpression.IsNullOrDefault:
            if (body.Type == typeof(string))
               be = Expression.Not(Expression.Call(null, GetIsNullOrEmptyMethod(), body));
            else
            {
               var canBeNull = !body.Type.IsValueType || (Nullable.GetUnderlyingType(body.Type) != null);
               be = canBeNull
                  ? Expression.AndAlso(Expression.NotEqual(body, Expression.Constant(null)),
                     Expression.NotEqual(body, Expression.Default(body.Type)))
                  : Expression.NotEqual(body, Expression.Default(body.Type));
            }

            break;
         case SyntaxKind.GreaterThan when areBothStrings == false:
            be = Expression.GreaterThan(body, GetValueExpression(body.Type, value));
            break;
         case SyntaxKind.LessThan when areBothStrings == false:
            be = Expression.LessThan(body, GetValueExpression(body.Type, value));
            break;
         case SyntaxKind.GreaterOrEqualThan when areBothStrings == false:
            be = Expression.GreaterThanOrEqual(body, GetValueExpression(body.Type, value));
            break;
         case SyntaxKind.LessOrEqualThan when areBothStrings == false:
            be = Expression.LessThanOrEqual(body, GetValueExpression(body.Type, value));
            break;
         case SyntaxKind.GreaterThan when areBothStrings:
            be = GetGreaterThanExpression(body, valueExpression, value);
            break;
         case SyntaxKind.LessThan when areBothStrings:
            be = GetLessThanExpression(body, valueExpression, value);
            break;
         case SyntaxKind.GreaterOrEqualThan when areBothStrings:
            be = GetGreaterThanOrEqualExpression(body, valueExpression, value);
            break;
         case SyntaxKind.LessOrEqualThan when areBothStrings:
            be = GetLessThanOrEqualExpression(body, valueExpression, value);
            break;
         case SyntaxKind.Like:
            be = Expression.Call(body, GetContainsMethod(), GetValueExpression(body.Type, value));
            break;
         case SyntaxKind.NotLike:
            be = Expression.Not(Expression.Call(body, GetContainsMethod(), GetValueExpression(body.Type, value)));
            break;
         case SyntaxKind.StartsWith:
            if (body.Type != typeof(string))
            {
               body = Expression.Call(body, GetToStringMethod());
               be = Expression.Call(body, GetStartWithMethod(), GetValueExpression(body.Type, value?.ToString()));
            }
            else
               be = Expression.Call(body, GetStartWithMethod(), GetValueExpression(body.Type, value));

            break;
         case SyntaxKind.EndsWith:
            if (body.Type != typeof(string))
            {
               body = Expression.Call(body, GetToStringMethod());
               be = Expression.Call(body, GetEndsWithMethod(), GetValueExpression(body.Type, value?.ToString()));
            }
            else
               be = Expression.Call(body, GetEndsWithMethod(), GetValueExpression(body.Type, value));

            break;
         case SyntaxKind.NotStartsWith:
            if (body.Type != typeof(string))
            {
               body = Expression.Call(body, GetToStringMethod());
               be = Expression.Not(Expression.Call(body, GetStartWithMethod(), GetValueExpression(body.Type, value?.ToString())));
            }
            else
               be = Expression.Not(Expression.Call(body, GetStartWithMethod(), GetValueExpression(body.Type, value)));

            break;
         case SyntaxKind.NotEndsWith:
            if (body.Type != typeof(string))
            {
               body = Expression.Call(body, GetToStringMethod());
               be = Expression.Not(Expression.Call(body, GetEndsWithMethod(), GetValueExpression(body.Type, value?.ToString())));
            }
            else
               be = Expression.Not(Expression.Call(body, GetEndsWithMethod(), GetValueExpression(body.Type, value)));

            break;
         case SyntaxKind.CustomOperator:
            var token = op as SyntaxToken;
            var customOperator = GridifyGlobalConfiguration.CustomOperators.Operators.First(q => q.GetOperator() == token!.Text);
            var customExp = customOperator.OperatorHandler();

            // replace prop parameter
            be = new ReplaceExpressionVisitor(customExp.Parameters[0], body).Visit(customExp.Body);

            var valueType = value?.GetType();
            be = new ReplaceExpressionVisitor(customExp.Parameters[1], GetValueExpression(valueType ?? typeof(object), value)).Visit(be);

            break;
         default:
            return null;
      }

      return Expression.Lambda(be, parameter);
   }

   protected override Expression<Func<T, bool>> CombineWithAndOperator(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
   {
      return left.And(right);
   }

   protected override Expression<Func<T, bool>> CombineWithOrOperator(Expression<Func<T, bool>> left, Expression<Func<T, bool>> right)
   {
      return left.Or(right);
   }

   private Expression<Func<T, bool>>? GenerateNestedExpression(Expression body, IGMap<T> gMap, ValueExpressionSyntax value, SyntaxNode op)
   {
      while (true)
      {
         switch (body)
         {
            case MethodCallExpression { Method.Name: "Select" } selectExp:
            {
               var targetExp = selectExp.Arguments.Single(a => a.NodeType == ExpressionType.Lambda) as LambdaExpression;
               var conditionExp = BuildQuery(targetExp!.Body, targetExp.Parameters[0], value, op, gMap.Convertor);

               if (conditionExp is not LambdaExpression lambdaExp) return null;

               return ParseMethodCallExpression(selectExp, lambdaExp) as Expression<Func<T, bool>>;
            }
            case ConditionalExpression cExp:
            {
               var ifTrue = GenerateNestedExpression(cExp.IfTrue, gMap, value, op);
               ifTrue = new ReplaceExpressionVisitor(ifTrue!.Parameters[0], gMap.To.Parameters[0]).Visit(ifTrue) as Expression<Func<T, bool>>;
               var ifFalse = GenerateNestedExpression(cExp.IfFalse, gMap, value, op);
               ifFalse = new ReplaceExpressionVisitor(ifFalse!.Parameters[0], gMap.To.Parameters[0]).Visit(ifFalse) as Expression<Func<T, bool>>;

               var newExp = Expression.Condition(cExp.Test, ifTrue!.Body, ifFalse!.Body);
               return Expression.Lambda<Func<T, bool>>(newExp, gMap.To.Parameters[0]);
            }
            case ConstantExpression constantExpression:
            {
               return Expression.Lambda<Func<T, bool>>(constantExpression, gMap.To.Parameters[0]);
            }
            case UnaryExpression uExp:
            {
               body = uExp.Operand;
               continue;
            }
            default:
               // this should never happening
               throw new GridifyFilteringException($"The 'Select' method on '{gMap.From}' for type {body.Type} not found");
         }
      }
   }

   private LambdaExpression ParseMethodCallExpression(MethodCallExpression exp, LambdaExpression predicate)
   {
      switch (exp.Arguments.First())
      {
         case MemberExpression member:
            return GetAnyExpression(member, predicate);
         case MethodCallExpression { Method.Name: "SelectMany" } subExp
            when subExp.Arguments.Last()
               is LambdaExpression { Body: MemberExpression lambdaMember }:
         {
            var newPredicate = GetAnyExpression(lambdaMember, predicate);
            return ParseMethodCallExpression(subExp, newPredicate);
         }
         case MethodCallExpression { Method.Name: "Select" } subExp
            when subExp.Arguments.Last() is LambdaExpression
            {
               Body: MemberExpression lambdaMember
            } lambda:
         {
            var newExp = new ReplaceExpressionVisitor(predicate.Parameters[0], lambdaMember).Visit(predicate.Body);
            var newPredicate = GetExpressionWithNullCheck(lambdaMember, lambda.Parameters[0], newExp);
            return ParseMethodCallExpression(subExp, newPredicate);
         }
         default:
            throw new InvalidOperationException();
      }
   }

   private ParameterExpression GetParameterExpression(MemberExpression member)
   {
      return member.Expression switch
      {
         ParameterExpression => Expression.Parameter(member.Expression!.Type, member.Expression.ToString()),
         MemberExpression subExp => GetParameterExpression(subExp),
         _ => throw new InvalidOperationException($"Invalid expression '{member.Expression}'")
      };
   }

   private MemberExpression GetPropertyOrField(MemberExpression member, ParameterExpression param)
   {
      return member.Expression switch
      {
         ParameterExpression => Expression.PropertyOrField(param, member.Member.Name),
         MemberExpression subExp => Expression.PropertyOrField(GetPropertyOrField(subExp, param), member.Member.Name),
         _ => throw new InvalidOperationException($"Invalid expression '{member.Expression}'")
      };
   }

   private LambdaExpression GetAnyExpression(MemberExpression member, Expression predicate)
   {
      var param = GetParameterExpression(member);
      var prop = GetPropertyOrField(member, param);

      var tp = prop.Type.IsGenericType
         ? prop.Type.GenericTypeArguments.First() // list
         : prop.Type.GetElementType(); // array

      if (tp == null) throw new GridifyFilteringException($"Can not detect the '{member.Member.Name}' property type.");

      var anyMethod = GetAnyMethod(tp);
      var anyExp = Expression.Call(anyMethod, prop, predicate);

      return GetExpressionWithNullCheck(prop, param, anyExp);
   }

   private LambdaExpression GetExpressionWithNullCheck(MemberExpression prop, ParameterExpression param, Expression right)
   {
      // This check is needed for EF6 - It doesn't support NullChecking for Collections (issue #58)
      // also issue #70 for NHibernate
      if (GridifyGlobalConfiguration.DisableNullChecks ||
          GridifyGlobalConfiguration.EntityFrameworkCompatibilityLayer)
         return Expression.Lambda(right, param);

      var nullChecker = Expression.NotEqual(prop, Expression.Constant(null));
      var exp = Expression.AndAlso(nullChecker, right);
      return Expression.Lambda(exp, param);
   }

   private Expression GetValueExpression(Type type, object? value)
   {
      if (!GridifyGlobalConfiguration.EntityFrameworkCompatibilityLayer)
         return Expression.Constant(value, type);

      // active parameterized query for EF
      const string fieldName = "Value";
      var (instance, type1) = GridifyTypeBuilder.CreateNewObject(type, fieldName, value);
      return Expression.PropertyOrField(Expression.Constant(instance, type1), fieldName);
   }

   private BinaryExpression GetLessThanOrEqualExpression(Expression body, ValueExpressionSyntax valueExpression, object? value)
   {
      if (GridifyGlobalConfiguration.EntityFrameworkCompatibilityLayer)
         return Expression.LessThanOrEqual(Expression.Call(null, GetCompareMethod(), body, GetValueExpression(typeof(string), value)),
            Expression.Constant(0));

      return Expression.LessThanOrEqual(Expression.Call(null, GetCompareMethodWithStringComparison(), body,
         GetValueExpression(typeof(string), value),
         GetStringComparisonCaseExpression(valueExpression.IsCaseInsensitive)), Expression.Constant(0));
   }

   private BinaryExpression GetGreaterThanOrEqualExpression(Expression body, ValueExpressionSyntax valueExpression, object? value)
   {
      if (GridifyGlobalConfiguration.EntityFrameworkCompatibilityLayer)
         return Expression.GreaterThanOrEqual(Expression.Call(null, GetCompareMethod(), body, GetValueExpression(typeof(string), value)),
            Expression.Constant(0));

      return Expression.GreaterThanOrEqual(Expression.Call(null, GetCompareMethodWithStringComparison(), body,
         GetValueExpression(typeof(string), value),
         GetStringComparisonCaseExpression(valueExpression.IsCaseInsensitive)), Expression.Constant(0));
   }

   private BinaryExpression GetLessThanExpression(Expression body, ValueExpressionSyntax valueExpression, object? value)
   {
      if (GridifyGlobalConfiguration.EntityFrameworkCompatibilityLayer)
         return Expression.LessThan(Expression.Call(null, GetCompareMethod(), body, GetValueExpression(typeof(string), value)),
            Expression.Constant(0));

      return Expression.LessThan(Expression.Call(null, GetCompareMethodWithStringComparison(), body,
         GetValueExpression(typeof(string), value),
         GetStringComparisonCaseExpression(valueExpression.IsCaseInsensitive)), Expression.Constant(0));
   }

   private BinaryExpression GetGreaterThanExpression(Expression body, ValueExpressionSyntax valueExpression, object? value)
   {
      if (GridifyGlobalConfiguration.EntityFrameworkCompatibilityLayer)
         return Expression.GreaterThan(Expression.Call(null, GetCompareMethod(), body, GetValueExpression(typeof(string), value)),
            Expression.Constant(0));

      return Expression.GreaterThan(Expression.Call(null, GetCompareMethodWithStringComparison(), body,
         GetValueExpression(typeof(string), value),
         GetStringComparisonCaseExpression(valueExpression.IsCaseInsensitive)), Expression.Constant(0));
   }

   private ConstantExpression GetStringComparisonCaseExpression(bool isCaseInsensitive)
   {
      return isCaseInsensitive
         ? Expression.Constant(StringComparison.OrdinalIgnoreCase)
         : Expression.Constant(StringComparison.Ordinal);
   }

   private MethodInfo GetAnyMethod(Type @type) =>
      typeof(Enumerable).GetMethods().Single(m => m.Name == "Any" && m.GetParameters().Length == 2).MakeGenericMethod(@type);

   private MethodInfo GetEndsWithMethod() => typeof(string).GetMethod("EndsWith", new[] { typeof(string) })!;

   private MethodInfo GetStartWithMethod() => typeof(string).GetMethod("StartsWith", new[] { typeof(string) })!;

   private MethodInfo GetContainsMethod() => typeof(string).GetMethod("Contains", new[] { typeof(string) })!;

   private MethodInfo GetIsNullOrEmptyMethod() => typeof(string).GetMethod("IsNullOrEmpty", new[] { typeof(string) })!;

   private MethodInfo GetCompareMethodWithStringComparison() =>
      typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string), typeof(StringComparison) })!;

   private MethodInfo GetCompareMethod() =>
      typeof(string).GetMethod("Compare", new[] { typeof(string), typeof(string) })!;

   private MethodInfo GetToStringMethod() => typeof(object).GetMethod("ToString")!;

   private MethodCallExpression ParseNestedExpression(Expression exp)
   {
      return exp switch
      {
         BinaryExpression { Right: MethodCallExpression cExp } => cExp,
         MethodCallExpression mcExp => mcExp,
         _ => throw new InvalidExpressionException()
      };
   }
}
