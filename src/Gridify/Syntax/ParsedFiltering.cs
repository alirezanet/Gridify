namespace Gridify.Syntax;

using System;

public sealed class ParsedFiltering
{
   public string MemberName { get; set; } = string.Empty;

   public string? Indexer { get; set; }
}
