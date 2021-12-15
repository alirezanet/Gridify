namespace Gridify;

public record GridifyMapperConfiguration
{
   /// <summary>
   /// Make Mappings case Sensitive
   /// Default is false
   /// </summary>
   public bool CaseSensitive { get; set; } = GridifyGlobalConfiguration.CaseSensitiveMappings;

   /// <summary>
   /// This option enables the 'null' keyword in filtering operations
   /// Default is true
   /// </summary>
   public bool AllowNullSearch { get; set; } = GridifyGlobalConfiguration.AllowNullSearch;

   /// <summary>
   /// If true, in filtering and ordering operations,
   /// gridify doesn't return any exceptions when a mapping
   /// is not defined for the fields
   /// Default is false
   /// </summary>
   public bool IgnoreNotMappedFields { get; set; } = GridifyGlobalConfiguration.IgnoreNotMappedFields;
}

