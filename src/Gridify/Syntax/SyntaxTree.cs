using System;
using System.Collections.Generic;
using System.Linq;

namespace Gridify.Syntax;

public sealed class SyntaxTree(IEnumerable<string> diagnostics, ExpressionSyntax root)
{
   private static readonly char[] OrderingSeparator = [' ', '\t'];

   public IEnumerable<string> Diagnostics { get; } = diagnostics;
   public ExpressionSyntax Root { get; } = root;


   public static SyntaxTree Parse(string text, IEnumerable<IGridifyOperator>? customOperators = null!)
   {
      customOperators ??= Enumerable.Empty<IGridifyOperator>();
      var parser = new Parser(text, customOperators);
      return parser.Parse();
   }

   /// <summary>
   /// Parses a string of filterings into a list of <see cref="ParsedFiltering"/> objects.
   /// </summary>
   /// <param name="filterings">A string of filterings.</param>
   /// <exception cref="GridifyFilteringException">Thrown when an invalid keyword is encountered in the filtering string.</exception>
   /// <returns>An <see cref="IEnumerable{ParsedFiltering}"/> representing the parsed filtering.</returns>
   public static IEnumerable<ParsedFiltering> ParseFilterings(string filterings)
   {
      if (string.IsNullOrWhiteSpace(filterings))
         yield break;

      var syntaxTree = Parse(filterings, GridifyGlobalConfiguration.CustomOperators.Operators);

      if (syntaxTree.Diagnostics.Any())
         throw new GridifyFilteringException(string.Join("\n", syntaxTree.Diagnostics.Reverse()));


      foreach (var fieldExpression in syntaxTree.Root.DistinctFieldExpressions().OrderBy(x => x.FieldToken.Position))
      {
         yield return new ParsedFiltering(fieldExpression.FieldToken.Text, fieldExpression.Indexer);
      }
   }

   /// <summary>
   /// Parses a string of orderings into a list of <see cref="ParsedOrdering"/> objects.
   /// </summary>
   /// <param name="orderings">A comma-separated string of orderings. Each ordering can optionally specify the sort direction (asc or desc).</param>
   /// <exception cref="GridifyOrderingException">Thrown when an invalid keyword is encountered in the orderings string.</exception>
   /// <returns>An <see cref="IEnumerable{ParsedOrdering}"/> representing the parsed orderings.</returns>
   public static IEnumerable<ParsedOrdering> ParseOrderings(string orderings)
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
