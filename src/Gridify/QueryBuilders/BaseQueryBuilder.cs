using System;
using System.ComponentModel;
using System.Linq.Expressions;
using System.Reflection;
using Gridify.Syntax;

namespace Gridify.QueryBuilders;

internal abstract class BaseQueryBuilder<TQuery, T>
   where TQuery : class
{
   protected readonly IGridifyMapper<T> mapper;

   protected BaseQueryBuilder(IGridifyMapper<T> mapper)
   {
      this.mapper = mapper;
   }

   internal TQuery Build(ExpressionSyntax expression)
   {
      var (query, _) = BuildQuery(expression);
      return query;
   }

   protected abstract TQuery? BuildNestedQuery(
      Expression body, IGMap<T> gMap, ValueExpressionSyntax value, SyntaxNode op);

   protected abstract TQuery BuildAlwaysTrueQuery();

   protected abstract TQuery BuildAlwaysFalseQuery(ParameterExpression parameter);

   protected abstract TQuery? CheckIfCanMergeQueries(
      (TQuery query, bool isNested) leftQuery,
      (TQuery query, bool isNested) rightQuery,
      SyntaxKind op);

   protected abstract object? BuildQueryAccordingToValueType(
      Expression body,
      ParameterExpression parameter,
      object? value,
      SyntaxNode op,
      ValueExpressionSyntax valueExpression,
      bool isConvertable);

   protected abstract TQuery CombineWithAndOperator(TQuery left, TQuery right);

   protected abstract TQuery CombineWithOrOperator(TQuery left, TQuery right);

   private (TQuery Query, bool IsNested) BuildQuery(ExpressionSyntax expression, bool isParenthesisOpen = false)
   {
      while (true)
         switch (expression.Kind)
         {
            case SyntaxKind.BinaryExpression:
            {
               var bExp = expression as BinaryExpressionSyntax;

               if (bExp!.Left is FieldExpressionSyntax && bExp.Right is ValueExpressionSyntax)
               {
                  try
                  {
                     return ConvertBinaryExpressionSyntaxToQuery(bExp)
                            ?? throw new GridifyFilteringException("Invalid expression");
                  }
                  catch (GridifyMapperException)
                  {
                     if (mapper.Configuration.IgnoreNotMappedFields)
                        return (BuildAlwaysTrueQuery(), false);

                     throw;
                  }
               }

               (TQuery query, bool isNested) leftQuery;
               (TQuery query, bool isNested) rightQuery;

               if (bExp.Left is ParenthesizedExpressionSyntax lpExp)
               {
                  leftQuery = BuildQuery(lpExp.Expression, true);
               }
               else
                  leftQuery = BuildQuery(bExp.Left);


               if (bExp.Right is ParenthesizedExpressionSyntax rpExp)
                  rightQuery = BuildQuery(rpExp.Expression, true);
               else
                  rightQuery = BuildQuery(bExp.Right);

               // check for nested collections
               if (isParenthesisOpen &&
                   CheckIfCanMergeQueries(leftQuery, rightQuery, bExp.OperatorToken.Kind) is { } mergedResult)
                  return (mergedResult, true);

               var result = bExp.OperatorToken.Kind switch
               {
                  SyntaxKind.And => CombineWithAndOperator(leftQuery.query, rightQuery.query),
                  SyntaxKind.Or => CombineWithOrOperator(leftQuery.query, rightQuery.query),
                  _ => throw new GridifyFilteringException($"Invalid expression Operator '{bExp.OperatorToken.Kind}'")
               };
               return (result, false);
            }
            case SyntaxKind.ParenthesizedExpression: // first entrypoint only
            {
               var pExp = expression as ParenthesizedExpressionSyntax;
               return BuildQuery(pExp!.Expression, true);
            }
            default:
               throw new GridifyFilteringException($"Invalid expression format '{expression.Kind}'.");
         }
   }

   private (TQuery, bool IsNested)? ConvertBinaryExpressionSyntaxToQuery(BinaryExpressionSyntax binarySyntax)
   {
      var fieldExpression = binarySyntax.Left as FieldExpressionSyntax;

      var left = fieldExpression?.FieldToken.Text.Trim();
      var right = (binarySyntax.Right as ValueExpressionSyntax);
      var op = binarySyntax.OperatorToken;

      if (left == null || right == null) return null;

      var gMap = mapper.GetGMap(left);

      if (gMap == null) throw new GridifyMapperException($"Mapping '{left}' not found");

      if (fieldExpression!.IsCollection)
         gMap.To = UpdateExpressionIndex(gMap.To, fieldExpression.Index);

      var isNested = ((GMap<T>)gMap).IsNestedCollection();
      if (isNested)
      {
         var result = BuildNestedQuery(gMap.To.Body, gMap, right, op);
         if (result == null) return null;
         return (result, isNested);
      }

      var query = BuildQuery(gMap.To.Body, gMap.To.Parameters[0], right, op, gMap.Convertor) as TQuery;
      if (query == null) return null;
      return (query, false);
   }

   protected object? BuildQuery(
      Expression body,
      ParameterExpression parameter,
      ValueExpressionSyntax valueExpression,
      SyntaxNode op,
      Func<string, object>? convertor)
   {
      // Remove the boxing for value types
      if (body.NodeType == ExpressionType.Convert) body = ((UnaryExpression)body).Operand;

      object? value = valueExpression.ValueToken.Text;

      // execute user custom Convertor
      if (convertor != null)
         value = convertor.Invoke(valueExpression.ValueToken.Text);

      // handle the `null` keyword in value
      if (mapper.Configuration.AllowNullSearch && op.Kind is SyntaxKind.Equal or SyntaxKind.NotEqual && value.ToString() == "null")
         value = null;

      var isConvertable = true;

      // type fixer
      // Check if body.Type is a nullable type and get its underlying type issue #134
      var underlyingBodyType = Nullable.GetUnderlyingType(body.Type);
      if (value is not null && (underlyingBodyType ?? body.Type) != value.GetType())
      {
         // handle bool, github issue #71
         if (body.Type == typeof(bool) && value is "true" or "false" or "1" or "0")
            value = ((string)value).ToLower() is "1" or "true";
         // handle broken guids,  github issue #2
         else if (body.Type == typeof(Guid) && !Guid.TryParse(value.ToString(), out _)) value = Guid.NewGuid().ToString();

         var converter = TypeDescriptor.GetConverter(body.Type);
         isConvertable = converter.CanConvertFrom(typeof(string));
         if (isConvertable)
         {
            try
            {
               value = converter.ConvertFromString(value.ToString()!);
            }
            catch (ArgumentException)
            {
               isConvertable = false;
            }
            catch (FormatException)
            {
               return BuildAlwaysFalseQuery(parameter);
            }
         }
      }

      // handle case-Insensitive search
      if (value is not null && valueExpression.IsCaseInsensitive
                            && op.Kind is not SyntaxKind.GreaterThan
                            && op.Kind is not SyntaxKind.LessThan
                            && op.Kind is not SyntaxKind.GreaterOrEqualThan
                            && op.Kind is not SyntaxKind.LessOrEqualThan)
      {
         value = value.ToString()?.ToLower();
         body = Expression.Call(body, GetToLowerMethod());
      }

      var query = BuildQueryAccordingToValueType(body, parameter, value, op, valueExpression, isConvertable);
      return query;
   }

   private static LambdaExpression UpdateExpressionIndex(LambdaExpression exp, int index)
   {
      var body = new ReplaceExpressionVisitor(exp.Parameters[1], Expression.Constant(index, typeof(int))).Visit(exp.Body);
      return Expression.Lambda(body, exp.Parameters);
   }

   private static MethodInfo GetToLowerMethod() => typeof(string).GetMethod("ToLower", new Type[] { })!;
}
