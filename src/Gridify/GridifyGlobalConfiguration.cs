using System;
using Gridify.Syntax;

namespace Gridify
{
   public static class GridifyGlobalConfiguration
   {
      /// <summary>
      /// It makes the generated expressions compatible
      /// whit the entity framework.
      /// Default is false.
      /// </summary>
      public static bool EntityFrameworkCompatibilityLayer { get; set; }

      /// <summary>
      /// Default page size for Gridify and Paging methods
      /// if no page size is specified
      /// </summary>
      public static int DefaultPageSize { get; set; } = 20;

      /// <summary>
      /// Make Mappings case Sensitive
      /// Default is false
      /// </summary>
      public static bool CaseSensitiveMapper { get; set; }

      /// <summary>
      /// This option enables the 'null' keyword in filtering operations
      /// Default is true
      /// </summary>
      public static bool AllowNullSearch { get; set; } = true;

      /// <summary>
      /// If true, in filtering and ordering operations,
      /// gridify doesn't return any exceptions when a mapping
      /// is not defined for the fields
      /// Default is false
      /// </summary>
      public static bool IgnoreNotMappedFields { get; set; }

      /// <summary>
      /// On nested collections by default gridify adds null check condition
      /// to prevent null reference exceptions, e.g () => field != null && field....
      /// some ORMs like NHibernate don't support this.
      /// you can disable this behavior by setting this option to true
      /// Default is false
      /// </summary>
      public static bool DisableNullChecks { get; set; } = false;

      /// <summary>
      /// Specifies how field names are inferred from CLR property names.
      /// By default, Elastic.Clients.Elasticsearch uses camel-case property names.
      /// </summary>
      /// <example>
      /// If false CLR property EmailAddress will be inferred as "emailAddress" Elasticsearch document field name
      /// If true, the CLR property EmailAddress will be inferred as "EmailAddress" Elasticsearch document field name
      /// </example>
      public static Func<string, string>? CustomElasticsearchNamingAction { get; set; }

      /// <summary>
      /// You can extend the gridify supported operators by adding
      /// your own operators to OperatorManager.
      /// Custom operators must implement the IGridifyOperator interface
      /// and must start with '#' character.
      /// </summary>
      public static OperatorManager CustomOperators { get; } = new();

      /// <summary>
      /// It Enables the EntityFramework Compatibility layer to
      /// make the generated expressions compatible whit entity framework.
      /// by default the Compatibility Layer is disabled.
      /// </summary>
      public static void EnableEntityFrameworkCompatibilityLayer()
      {
         EntityFrameworkCompatibilityLayer = true;
      }

      /// <summary>
      /// It Disables the EntityFramework Compatibility layer
      /// by default the Compatibility Layer is disabled.
      /// </summary>
      public static void DisableEntityFrameworkCompatibilityLayer()
      {
         EntityFrameworkCompatibilityLayer = false;
      }
   }
}
