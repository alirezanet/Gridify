using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Gridify.QueryBuilders;
using Gridify.Syntax;

namespace Gridify.Elasticsearch;

internal class ElasticsearchQueryBuilder<T> : BaseQueryBuilder<Query, T>
{
   public ElasticsearchQueryBuilder(IGridifyMapper<T> mapper) : base(mapper)
   {
   }

   protected override Query BuildNestedQuery(
      Expression body, IGMap<T> gMap, ValueExpressionSyntax value, SyntaxNode op)
   {
      throw new NotSupportedException();
   }

   protected override Query BuildAlwaysTrueQuery()
   {
      return new BoolQuery();
   }

   protected override Query BuildAlwaysFalseQuery(ParameterExpression parameter)
   {
      return new BoolQuery { MustNot = new List<Query> { new MatchAllQuery() } };
   }

   protected override Query? CheckIfCanMergeQueries(
      (Query query, bool isNested) leftQuery,
      (Query query, bool isNested) rightQuery,
      SyntaxKind op)
   {
      return null;
   }

   protected override object BuildQueryAccordingToValueType(
      Expression body,
      ParameterExpression parameter,
      object? value,
      SyntaxNode op,
      ValueExpressionSyntax valueExpression,
      bool isConvertable)
   {
      if (valueExpression.IsCaseInsensitive)
      {
         throw new NotSupportedException("Case insensitive filtering is not supported by Gridify.Elasticsearch");
      }

      bool isStringValue = false, isNumberExceptDecimalValue = false;
      if (IsString(value))
      {
         isStringValue = true;
      }
      else if (IsNumberExceptDecimal(value))
      {
         isNumberExceptDecimalValue = true;
      }

      var fieldName = body.BuildFieldName(isStringValue, mapper);
      var field = new Field(fieldName);
      var right = valueExpression.ValueToken.Text;

      Query query;
      switch (op.Kind)
      {
         case SyntaxKind.Equal when value is null:
            query = new BoolQuery { MustNot = new List<Query> { new ExistsQuery { Field = field } } };
            break;
         case SyntaxKind.Equal when value is DateTime dateTime:
            query = new TermQuery(field) { Value = ConvertToString(dateTime) };
            break;
         case SyntaxKind.Equal when isNumberExceptDecimalValue:
            query = new TermQuery(field) { Value = Convert.ToDouble(right) };
            break;
         case SyntaxKind.Equal when value is decimal decimalValue:
            query = new TermQuery(field) { Value = ConvertToString(decimalValue) };
            break;
         case SyntaxKind.Equal:
            query = new TermQuery(field) { Value = right };
            break;
         case SyntaxKind.NotEqual when value is null:
            query = new ExistsQuery { Field = field };
            break;
         case SyntaxKind.NotEqual when value is DateTime dateTime:
            query = new BoolQuery { MustNot = new List<Query> { new TermQuery(field) { Value = ConvertToString(dateTime) } } };
            break;
         case SyntaxKind.NotEqual when isNumberExceptDecimalValue:
            query = new BoolQuery { MustNot = new List<Query> { new TermQuery(field) { Value = Convert.ToDouble(right) } } };
            break;
         case SyntaxKind.NotEqual when value is decimal decimalValue:
            query = new BoolQuery { MustNot = new List<Query> { new TermQuery(field) { Value = ConvertToString(decimalValue) } } };
            break;
         case SyntaxKind.NotEqual:
            query = new BoolQuery { MustNot = new List<Query> { new TermQuery(field) { Value = right } } };
            break;
         case SyntaxKind.GreaterThan when value is DateTime dateTime:
            query = new DateRangeQuery(field) { Gt = dateTime };
            break;
         case SyntaxKind.GreaterThan when isNumberExceptDecimalValue:
            query = new NumberRangeQuery(field) { Gt = Convert.ToDouble(right) };
            break;
         case SyntaxKind.GreaterThan when value is decimal decimalValue:
            // NOTE: RangeQuery doesn't have a public constructor, so we use DateRangeQuery instead, that works correctly.
            query = new DateRangeQuery(field) { Gt = ConvertToString(decimalValue) };
            break;
         case SyntaxKind.GreaterThan:
            query = new DateRangeQuery(field) { Gt = right };
            break;
         case SyntaxKind.LessThan when value is DateTime dateTime:
            query = new DateRangeQuery(field) { Lt = dateTime };
            break;
         case SyntaxKind.LessThan when isNumberExceptDecimalValue:
            query = new NumberRangeQuery(field) { Lt = Convert.ToDouble(right) };
            break;
         case SyntaxKind.LessThan when value is decimal decimalValue:
            query = new DateRangeQuery(field) { Lt = ConvertToString(decimalValue) };
            break;
         case SyntaxKind.LessThan:
            query = new DateRangeQuery(field) { Lt = right };
            break;
         case SyntaxKind.GreaterOrEqualThan when value is DateTime dateTime:
            query = new DateRangeQuery(field) { Gte = dateTime };
            break;
         case SyntaxKind.GreaterOrEqualThan when isNumberExceptDecimalValue:
            query = new NumberRangeQuery(field) { Gte = Convert.ToDouble(right) };
            break;
         case SyntaxKind.GreaterOrEqualThan when value is decimal decimalValue:
            query = new DateRangeQuery(field) { Gte = ConvertToString(decimalValue) };
            break;
         case SyntaxKind.GreaterOrEqualThan:
            query = new DateRangeQuery(field) { Gte = right };
            break;
         case SyntaxKind.LessOrEqualThan when value is DateTime dateTime:
            query = new DateRangeQuery(field) { Lte = dateTime };
            break;
         case SyntaxKind.LessOrEqualThan when isNumberExceptDecimalValue:
            query = new NumberRangeQuery(field) { Lte = Convert.ToDouble(right) };
            break;
         case SyntaxKind.LessOrEqualThan when value is decimal decimalValue:
            query = new DateRangeQuery(field) { Lte = ConvertToString(decimalValue) };
            break;
         case SyntaxKind.LessOrEqualThan:
            query = new DateRangeQuery(field) { Lte = right };
            break;
         case SyntaxKind.Like:
            query = new WildcardQuery(field) { Value = $"*{right}*" };
            break;
         case SyntaxKind.NotLike:
            query = new BoolQuery { MustNot = new List<Query> { new WildcardQuery(field) { Value = $"*{right}*" } } };
            break;
         case SyntaxKind.StartsWith:
            query = new WildcardQuery(field) { Value = $"{right}*" };
            break;
         case SyntaxKind.EndsWith:
            query = new WildcardQuery(field) { Value = $"*{right}" };
            break;
         case SyntaxKind.NotStartsWith:
            query = new BoolQuery { MustNot = new List<Query> { new WildcardQuery(field) { Value = $"{right}*" } } };
            break;
         case SyntaxKind.NotEndsWith:
            query = new BoolQuery { MustNot = new List<Query> { new WildcardQuery(field) { Value = $"*{right}" } } };
            break;
         case SyntaxKind.CustomOperator:
            throw new NotImplementedException();
         default:
            throw new GridifyFilteringException("Invalid expression");
      }

      return query;
   }

   protected override Query CombineWithAndOperator(Query left, Query right)
   {
      return left & right;
   }

   protected override Query CombineWithOrOperator(Query left, Query right)
   {
      return left | right;
   }

   private static bool IsString(object? value)
   {
      return value?.GetType() == typeof(string) || value?.GetType() == typeof(Guid);
   }

   private static bool IsNumberExceptDecimal(object? value)
   {
      return value
         is byte
         or sbyte
         or short
         or ushort
         or int
         or uint
         or long
         or ulong
         or double
         or float;
   }

   private static string ConvertToString(DateTime dateTime)
   {
      return dateTime.ToString("yyyy-MM-ddTHH:mm:ss");
   }

   private static string ConvertToString(decimal decimalValue)
   {
      return decimalValue.ToString("0.0#################");
   }
}
