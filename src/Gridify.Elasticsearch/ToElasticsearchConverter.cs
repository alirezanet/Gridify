using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Text.Json;
using Elastic.Clients.Elasticsearch;
using Elastic.Clients.Elasticsearch.QueryDsl;
using Gridify.Syntax;

namespace Gridify.Elasticsearch;

internal static class ToElasticsearchConverter
{
   internal static Query GenerateQuery<T>(ExpressionSyntax expression, IGridifyMapper<T> mapper)
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
                     return ConvertBinaryExpressionSyntaxToQuery(bExp, mapper)
                            ?? throw new GridifyFilteringException("Invalid expression");
                  }
                  catch (GridifyMapperException)
                  {
                     if (mapper.Configuration.IgnoreNotMappedFields)
                        return new BoolQuery();

                     throw;
                  }
               }

               Query leftQuery;
               Query rightQuery;

               if (bExp.Left is ParenthesizedExpressionSyntax lpExp)
                  leftQuery = GenerateQuery(lpExp.Expression, mapper);
               else
                  leftQuery = GenerateQuery(bExp.Left, mapper);

               if (bExp.Right is ParenthesizedExpressionSyntax rpExp)
                  rightQuery = GenerateQuery(rpExp.Expression, mapper);
               else
                  rightQuery = GenerateQuery(bExp.Right, mapper);

               var result = bExp.OperatorToken.Kind switch
               {
                  SyntaxKind.And => leftQuery & rightQuery,
                  SyntaxKind.Or => leftQuery | rightQuery,
                  _ => throw new GridifyFilteringException($"Invalid expression Operator '{bExp.OperatorToken.Kind}'")
               };
               return (result);
            }
            case SyntaxKind.ParenthesizedExpression: // first entrypoint only
            {
               var pExp = expression as ParenthesizedExpressionSyntax;
               return GenerateQuery(pExp!.Expression, mapper);
            }
            default:
               throw new GridifyFilteringException($"Invalid expression format '{expression.Kind}'.");
         }
   }

   internal static ICollection<SortOptions> GenerateSortOptions<T>(List<ParsedOrdering> orderings, IGridifyMapper<T> mapper)
   {
      var sortOptions = new List<SortOptions>();
      foreach (var order in orderings)
      {
         if (!mapper.HasMap(order.MemberName))
         {
            // skip if there is no mappings available
            if (mapper.Configuration.IgnoreNotMappedFields)
               continue;

            throw new GridifyMapperException($"Mapping '{order.MemberName}' not found");
         }

         var propExpression = mapper.GetExpression(order.MemberName);
         var isStringValue = propExpression.GetRealType() == typeof(string);
         var fieldName = BuildFieldName(propExpression.Body, mapper.Configuration, isStringValue);

         var sortOption = SortOptions.Field(
            fieldName,
            new FieldSort { Order = order.IsAscending ? SortOrder.Asc : SortOrder.Desc });

         sortOptions.Add(sortOption);
      }

      return sortOptions;
   }

   private static Query? ConvertBinaryExpressionSyntaxToQuery<T>(BinaryExpressionSyntax binarySyntax, IGridifyMapper<T> mapper)
   {
      var fieldExpression = binarySyntax.Left as FieldExpressionSyntax;

      var left = fieldExpression?.FieldToken.Text.Trim();
      var right = binarySyntax.Right as ValueExpressionSyntax;
      var op = binarySyntax.OperatorToken;

      if (left == null || right == null) return null;

      var gMap = mapper.GetGMap(left);
      if (gMap == null) throw new GridifyMapperException($"Mapping '{left}' not found");

      if (fieldExpression!.IsCollection)
         throw new NotSupportedException();

      var isNested = ((GMap<T>)gMap).IsNestedCollection();
      if (isNested)
      {
         throw new NotSupportedException();
      }

      var result = GenerateQuery(
         gMap.To.Body,
         right,
         op,
         gMap.Convertor,
         right.ValueToken.Text,
         mapper.Configuration);

      return result;
   }

   private static Query GenerateQuery(
      Expression body,
      ValueExpressionSyntax valueExpression,
      SyntaxNode op,
      Func<string, object>? convertor,
      string right,
      GridifyMapperConfiguration mapperConfiguration)
   {
      // Remove the boxing for value types
      if (body.NodeType == ExpressionType.Convert) body = ((UnaryExpression)body).Operand;

      object? value = valueExpression.ValueToken.Text;

      // execute user custom Convertor
      if (convertor != null)
         value = convertor.Invoke(valueExpression.ValueToken.Text);

      // handle the `null` keyword in value
      if (mapperConfiguration.AllowNullSearch && op.Kind is SyntaxKind.Equal or SyntaxKind.NotEqual && value.ToString() == "null")
         value = null;

      // type fixer
      if (value is not null && body.Type != value.GetType())
      {
         try
         {
            // handle bool, github issue #71
            if (body.Type == typeof(bool) && value is "true" or "false" or "1" or "0")
               value = (((string)value).ToLower() is "1" or "true");
            // handle broken guids, github issue #2
            else if (body.Type == typeof(Guid) && !Guid.TryParse(value.ToString(), out _)) value = Guid.NewGuid().ToString();

            var converter = TypeDescriptor.GetConverter(body.Type);
            var isConvertable = converter.CanConvertFrom(typeof(string));
            if (isConvertable)
               value = converter.ConvertFromString(value.ToString()!);
         }
         catch (FormatException)
         {
            // this code should never run
            // return no records in case of any exception in formatting
            return new BoolQuery { MustNot = new List<Query> { new MatchAllQuery() } };
         }
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

      var fieldName = BuildFieldName(body, mapperConfiguration, isStringValue);
      var field = new Field(fieldName);

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
            throw new GridifyFilteringException("Invalid expression");;
      }

      return query;
   }

   private static string BuildFieldName(
      Expression expression, GridifyMapperConfiguration mapperConfiguration, bool isStringValue)
   {
      var propertyPath = expression.ToPropertyPath();
      var propertyPathParts = propertyPath.Split('.');
      propertyPath = string.Join(".", propertyPathParts.Select(
         mapperConfiguration.CustomElasticsearchNamingAction ?? JsonNamingPolicy.CamelCase.ConvertName));

      return isStringValue ? $"{propertyPath}.keyword" : propertyPath;
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
