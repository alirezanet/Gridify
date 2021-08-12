namespace Gridify
{
   public record GridifyMapperConfiguration
   {
      public bool CaseSensitive { get; set; }
      public bool AllowNullSearch { get; set; } = true;
   }
}