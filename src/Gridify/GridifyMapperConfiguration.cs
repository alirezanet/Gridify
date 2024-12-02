using System;

namespace Gridify;

public record GridifyMapperConfiguration
{
   /// <summary>
   /// Make Mappings case Sensitive
   /// Default is false
   /// </summary>
   public bool CaseSensitive { get; set; } = GridifyGlobalConfiguration.CaseSensitiveMapper;

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

   /// <summary>
   /// If true, string comparison operations are case-insensitive by default.
   /// Default is false
   /// </summary>
   public bool CaseInsensitiveFiltering { get; set; } = GridifyGlobalConfiguration.CaseInsensitiveFiltering;

   /// <summary>
   /// By default, DateTimeKind.Unspecified is used.
   /// You can change this behavior by setting this property to a DateTimeKind value.
   /// Default is null
   /// </summary>
   public DateTimeKind? DefaultDateTimeKind { get; set; } = GridifyGlobalConfiguration.DefaultDateTimeKind;

   /// <summary>
   /// On nested collections by default gridify adds null check condition
   /// to prevent null reference exceptions, e.g () => field != null && field....
   /// some ORMs like NHibernate don't support this.
   /// you can disable this behavior by setting this option to true
   /// Default is false
   /// </summary>
   public bool DisableCollectionNullChecks { get; set; } = GridifyGlobalConfiguration.DisableNullChecks;

   /// <summary>
   /// It makes the generated expressions compatible
   /// with the entity framework.
   /// Default is false.
   /// </summary>
   public bool EntityFrameworkCompatibilityLayer { get; set; } = GridifyGlobalConfiguration.EntityFrameworkCompatibilityLayer;

   /// <summary>
   /// Specifies how field names are inferred from CLR property names.
   /// By default, Elastic.Clients.Elasticsearch uses camel-case property names.
   /// </summary>
   /// <example>
   /// If null (default behavior) CLR property EmailAddress will be inferred as "emailAddress" Elasticsearch document field name.
   /// If, e.g., <c>p => p</c>, the CLR property EmailAddress will be inferred as "EmailAddress" Elasticsearch document field name.
   /// </example>
   public Func<string, string>? CustomElasticsearchNamingAction { get; set; } = GridifyGlobalConfiguration.CustomElasticsearchNamingAction;
}
