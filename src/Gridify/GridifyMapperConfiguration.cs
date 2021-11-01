namespace Gridify
{
   public record GridifyMapperConfiguration
   {
      public bool CaseSensitive { get; set; }
      
      /// <summary>
      /// This option enables the 'null' keyword in filtering operations
      /// </summary>
      public bool AllowNullSearch { get; set; } = true;
      /// <summary>
      /// If true, in filtering and ordering operations,
      /// gridify doesn't return any exceptions when a mapping
      /// is not defined for the fields
      /// </summary>
      public bool IgnoreNotMappedFields { get; set; }
   }
}