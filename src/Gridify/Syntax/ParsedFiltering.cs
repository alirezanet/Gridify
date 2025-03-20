namespace Gridify.Syntax;

using System;

public sealed class ParsedFiltering
{
   public ParsedFiltering(string memberName, string? indexer)
   {
      MemberName = memberName;
      Indexer = indexer;
   }

   public string MemberName { get; }

   public string? Indexer { get; }
}
