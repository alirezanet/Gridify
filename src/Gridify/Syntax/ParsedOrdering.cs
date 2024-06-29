namespace Gridify.Syntax;

public sealed class ParsedOrdering
{
   public string MemberName { get; set; } = string.Empty;
   public bool IsAscending { get; set; }

   public OrderingType OrderingType { get; set; } = OrderingType.Normal;
}

public enum OrderingType
{
   Normal,
   NullCheck,
   NotNullCheck
}
