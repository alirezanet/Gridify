using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Gridify.Syntax;

internal static class SelectTokenizer
{
   private static readonly Regex PathRegex = new(@"^[A-Za-z_][A-Za-z0-9_]*(\.[A-Za-z_][A-Za-z0-9_]*)*$", RegexOptions.Compiled);

   public static IReadOnlyList<string> Parse(string? input)
   {
      if (string.IsNullOrWhiteSpace(input))
         return Array.Empty<string>();

      var rawTokens = input!.Split(',');
      var seen = new HashSet<string>(StringComparer.Ordinal);
      var result = new List<string>(rawTokens.Length);

      foreach (var raw in rawTokens)
      {
         var token = raw.Trim();
         if (token.Length == 0)
            throw new GridifySelectException($"Invalid select syntax: empty token in '{input}'");

         if (!PathRegex.IsMatch(token))
            throw new GridifySelectException($"Invalid select syntax near '{token}'");

         if (seen.Add(token))
            result.Add(token);
      }

      return result;
   }
}
