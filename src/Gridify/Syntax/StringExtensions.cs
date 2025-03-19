using System;
using System.Collections.Generic;
using System.Linq;

namespace Gridify.Syntax
{
   public static class StringExtensions
   {
      private static readonly char[] OrderingSeparator = [' ', '\t'];

      /// <summary>
      /// Parses a string of filterings into a list of <see cref="ParsedFiltering"/> objects.
      /// </summary>
      /// <param name="filterings">A string of filterings.</param>
      /// <exception cref="GridifyFilteringException">Thrown when an invalid keyword is encountered in the filtering string.</exception>
      /// <returns>An <see cref="IEnumerable{ParsedFiltering}"/> representing the parsed filtering.</returns>
      public static IEnumerable<ParsedFiltering> ParseFilterings(this string filterings)
      {
         if (string.IsNullOrWhiteSpace(filterings))
            yield break;

         var parser = new Parser(filterings, GridifyGlobalConfiguration.CustomOperators.Operators);
         var syntaxTree = parser.Parse();

         if (syntaxTree.Diagnostics.Any())
            throw new GridifyFilteringException(string.Join("\n", syntaxTree.Diagnostics.Reverse()));


         foreach (var fieldExpression in syntaxTree.Root.DescendantsFieldExpressions().OrderBy(x => x.FieldToken.Position))
         {
            yield return new ParsedFiltering
            {
               MemberName = fieldExpression.FieldToken.Text,
               Indexer = fieldExpression.Indexer,
            };
         }
      }

      /// <summary>
      /// Parses a string of orderings into a list of <see cref="ParsedOrdering"/> objects.
      /// </summary>
      /// <param name="orderings">A comma-separated string of orderings. Each ordering can optionally specify the sort direction (asc or desc).</param>
      /// <exception cref="GridifyOrderingException">Thrown when an invalid keyword is encountered in the orderings string.</exception>
      /// <returns>An <see cref="IEnumerable{ParsedOrdering}"/> representing the parsed orderings.</returns>
      public static IEnumerable<ParsedOrdering> ParseOrderings(this string orderings)
      {
         if (string.IsNullOrWhiteSpace(orderings))
            yield break;

         var nullableChars = new[] { '?', '!' };
         foreach (var field in orderings.Split(','))
         {
            var orderingExp = field.Trim();
            if (orderingExp.Contains(' '))
            {
               var spliced = orderingExp.Split(OrderingSeparator, StringSplitOptions.RemoveEmptyEntries);
               var isAsc = spliced.Last() switch
               {
                  "desc" => false,
                  "asc" => true,
                  _ => throw new GridifyOrderingException("Invalid keyword. expected 'desc' or 'asc'")
               };
               var member = spliced.First();
               yield return new ParsedOrdering
               {
                  MemberName = member.ReplaceAll(nullableChars, ' ').TrimEnd(),
                  IsAscending = isAsc,
                  OrderingType = member.EndsWith("?") ? OrderingType.NullCheck
                     : member.EndsWith("!") ? OrderingType.NotNullCheck
                     : OrderingType.Normal
               };
            }
            else
            {
               yield return new ParsedOrdering
               {
                  MemberName = orderingExp.ReplaceAll(nullableChars, ' ').TrimEnd(),
                  IsAscending = true,
                  OrderingType = orderingExp.EndsWith("?") ? OrderingType.NullCheck
                     : orderingExp.EndsWith("!") ? OrderingType.NotNullCheck
                     : OrderingType.Normal
               };
            }
         }
      }

    
   }
}
