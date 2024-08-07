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
   /// By default, DateTimeKind.Unspecified is used.
   /// You can change this behavior by setting this property to a DateTimeKind value.
   /// Default is null
   /// </summary>
   public DateTimeKind? DefaultDateTimeKind { get; set; } = GridifyGlobalConfiguration.DefaultDateTimeKind;

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
