using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Gridify.Builder;
using Gridify.Syntax;

namespace Gridify.Elasticsearch;

public class ElasticsearchQueryBuilder<T>(IGridifyMapper<T> mapper) : BaseQueryBuilder<Query, T>(mapper)
{
   private readonly IGridifyMapper<T> _mapper = mapper;

   protected override Query BuildNestedQuery(
      Expression body, IGMap<T> gMap, ValueExpressionSyntax value, ISyntaxNode op)
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
      ISyntaxNode op,
      ValueExpressionSyntax valueExpression)
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

      var fieldName = body.BuildFieldName(isStringValue, _mapper);
      var field = new Field(fieldName);
      var right = valueExpression.ValueToken.Text;

      Query query = op.Kind switch
      {
         SyntaxKind.Equal when value is null => new BoolQuery { MustNot = new List<Query> { new ExistsQuery { Field = field } } },
         SyntaxKind.Equal when value is DateTime dateTime => new TermQuery(field) { Value = ConvertToString(dateTime) },
         SyntaxKind.Equal when isNumberExceptDecimalValue => new TermQuery(field) { Value = Convert.ToDouble(right) },
         SyntaxKind.Equal when value is decimal decimalValue => new TermQuery(field) { Value = ConvertToString(decimalValue) },
         SyntaxKind.Equal => new TermQuery(field) { Value = right },
         SyntaxKind.NotEqual when value is null => new ExistsQuery { Field = field },
         SyntaxKind.NotEqual when value is DateTime dateTime => new BoolQuery
         {
            MustNot = new List<Query> { new TermQuery(field) { Value = ConvertToString(dateTime) } }
         },
         SyntaxKind.NotEqual when isNumberExceptDecimalValue => new BoolQuery
         {
            MustNot = new List<Query> { new TermQuery(field) { Value = Convert.ToDouble(right) } }
         },
         SyntaxKind.NotEqual when value is decimal decimalValue => new BoolQuery
         {
            MustNot = new List<Query> { new TermQuery(field) { Value = ConvertToString(decimalValue) } }
         },
         SyntaxKind.NotEqual => new BoolQuery { MustNot = new List<Query> { new TermQuery(field) { Value = right } } },
         SyntaxKind.GreaterThan when value is DateTime dateTime => new DateRangeQuery(field) { Gt = dateTime },
         SyntaxKind.GreaterThan when isNumberExceptDecimalValue => new NumberRangeQuery(field) { Gt = Convert.ToDouble(right) },
         SyntaxKind.GreaterThan when value is decimal decimalValue =>
            // NOTE: RangeQuery doesn't have a public constructor, so we use DateRangeQuery instead, that works correctly.
            new DateRangeQuery(field) { Gt = ConvertToString(decimalValue) },
         SyntaxKind.GreaterThan => new DateRangeQuery(field) { Gt = right },
         SyntaxKind.LessThan when value is DateTime dateTime => new DateRangeQuery(field) { Lt = dateTime },
         SyntaxKind.LessThan when isNumberExceptDecimalValue => new NumberRangeQuery(field) { Lt = Convert.ToDouble(right) },
         SyntaxKind.LessThan when value is decimal decimalValue => new DateRangeQuery(field) { Lt = ConvertToString(decimalValue) },
         SyntaxKind.LessThan => new DateRangeQuery(field) { Lt = right },
         SyntaxKind.GreaterOrEqualThan when value is DateTime dateTime => new DateRangeQuery(field) { Gte = dateTime },
         SyntaxKind.GreaterOrEqualThan when isNumberExceptDecimalValue => new NumberRangeQuery(field) { Gte = Convert.ToDouble(right) },
         SyntaxKind.GreaterOrEqualThan when value is decimal decimalValue => new DateRangeQuery(field) { Gte = ConvertToString(decimalValue) },
         SyntaxKind.GreaterOrEqualThan => new DateRangeQuery(field) { Gte = right },
         SyntaxKind.LessOrEqualThan when value is DateTime dateTime => new DateRangeQuery(field) { Lte = dateTime },
         SyntaxKind.LessOrEqualThan when isNumberExceptDecimalValue => new NumberRangeQuery(field) { Lte = Convert.ToDouble(right) },
         SyntaxKind.LessOrEqualThan when value is decimal decimalValue => new DateRangeQuery(field) { Lte = ConvertToString(decimalValue) },
         SyntaxKind.LessOrEqualThan => new DateRangeQuery(field) { Lte = right },
         SyntaxKind.Like => new WildcardQuery(field) { Value = $"*{right}*" },
         SyntaxKind.NotLike => new BoolQuery { MustNot = new List<Query> { new WildcardQuery(field) { Value = $"*{right}*" } } },
         SyntaxKind.StartsWith => new WildcardQuery(field) { Value = $"{right}*" },
         SyntaxKind.EndsWith => new WildcardQuery(field) { Value = $"*{right}" },
         SyntaxKind.NotStartsWith => new BoolQuery { MustNot = new List<Query> { new WildcardQuery(field) { Value = $"{right}*" } } },
         SyntaxKind.NotEndsWith => new BoolQuery { MustNot = new List<Query> { new WildcardQuery(field) { Value = $"*{right}" } } },
         SyntaxKind.CustomOperator => throw new NotSupportedException("Custom operators are not supported in the Gridify.Elasticsearch extension"),
         _ => throw new GridifyFilteringException("Invalid expression")
      };

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
