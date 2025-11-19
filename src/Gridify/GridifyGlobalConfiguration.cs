using System;
using System.Threading;
using Gridify.Syntax;

namespace Gridify
{
   /// <summary>
   /// Thread-safe global configuration for Gridify using AsyncLocal for proper isolation
   /// in concurrent and test scenarios.
   /// </summary>
   public static class GridifyGlobalConfiguration
   {
      // AsyncLocal storage for thread-safe configuration values
      private static readonly AsyncLocal<bool?> _entityFrameworkCompatibilityLayer = new();
      private static readonly AsyncLocal<int?> _defaultPageSize = new();
      private static readonly AsyncLocal<bool?> _caseSensitiveMapper = new();
      private static readonly AsyncLocal<bool?> _allowNullSearch = new();
      private static readonly AsyncLocal<bool?> _ignoreNotMappedFields = new();
      private static readonly AsyncLocal<bool?> _avoidNullReference = new();
      private static readonly AsyncLocal<bool?> _disableNullChecks = new();
      private static readonly AsyncLocal<bool?> _caseInsensitiveFiltering = new();
      private static readonly AsyncLocal<DateTimeKind?> _defaultDateTimeKind = new();
      private static readonly AsyncLocal<Func<string, string>?> _customElasticsearchNamingAction = new();

      // CustomOperators needs special handling - using ThreadLocal with a factory
      private static readonly ThreadLocal<OperatorManager?> _customOperators = new(() => null);
      private static readonly Lazy<OperatorManager> _globalCustomOperators = new(() => new OperatorManager());

      /// <summary>
      /// It makes the generated expressions compatible
      /// with the entity framework.
      /// Default is false.
      /// </summary>
      public static bool EntityFrameworkCompatibilityLayer
      {
         get => _entityFrameworkCompatibilityLayer.Value ?? false;
         set => _entityFrameworkCompatibilityLayer.Value = value;
      }

      /// <summary>
      /// Default page size for Gridify and Paging methods
      /// if no page size is specified
      /// </summary>
      public static int DefaultPageSize
      {
         get => _defaultPageSize.Value ?? 20;
         set => _defaultPageSize.Value = value;
      }

      /// <summary>
      /// Make Mappings case Sensitive
      /// Default is false
      /// </summary>
      public static bool CaseSensitiveMapper
      {
         get => _caseSensitiveMapper.Value ?? false;
         set => _caseSensitiveMapper.Value = value;
      }

      /// <summary>
      /// This option enables the 'null' keyword in filtering operations
      /// Default is true
      /// </summary>
      public static bool AllowNullSearch
      {
         get => _allowNullSearch.Value ?? true;
         set => _allowNullSearch.Value = value;
      }

      /// <summary>
      /// If true, in filtering and ordering operations,
      /// gridify doesn't return any exceptions when a mapping
      /// is not defined for the fields
      /// Default is false
      /// </summary>
      public static bool IgnoreNotMappedFields
      {
         get => _ignoreNotMappedFields.Value ?? false;
         set => _ignoreNotMappedFields.Value = value;
      }

      /// <summary>
      /// This option allows for an intermediate object to be null i.e in obj.PropA.Prob PropA can be null
      /// This configuration is specific for properties and was introduced after DisableNullChecks.
      /// Hence it has its own property.
      /// </summary>
      public static bool AvoidNullReference
      {
         get => _avoidNullReference.Value ?? false;
         set => _avoidNullReference.Value = value;
      }

      /// <summary>
      /// On nested collections by default gridify adds null check condition
      /// to prevent null reference exceptions, e.g () => field != null && field....
      /// some ORMs like NHibernate don't support this.
      /// you can disable this behavior by setting this option to true
      /// Default is false
      /// </summary>
      public static bool DisableNullChecks
      {
         get => _disableNullChecks.Value ?? false;
         set => _disableNullChecks.Value = value;
      }

      /// <summary>
      /// By default, string comparison is case-sensitive.
      /// You can change this behavior by setting this property to true.
      /// Default is false
      /// </summary>
      public static bool CaseInsensitiveFiltering
      {
         get => _caseInsensitiveFiltering.Value ?? false;
         set => _caseInsensitiveFiltering.Value = value;
      }

      /// <summary>
      /// By default, DateTimeKind.Unspecified is used.
      /// You can change this behavior by setting this property to a DateTimeKind value.
      /// Default is null
      /// </summary>
      public static DateTimeKind? DefaultDateTimeKind
      {
         get => _defaultDateTimeKind.Value;
         set => _defaultDateTimeKind.Value = value;
      }

      /// <summary>
      /// Specifies how field names are inferred from CLR property names.
      /// By default, Elastic.Clients.Elasticsearch uses camel-case property names.
      /// </summary>
      /// <example>
      /// If null (default behavior) CLR property EmailAddress will be inferred as "emailAddress" Elasticsearch document field name.
      /// If, e.g., <c>p => p</c>, the CLR property EmailAddress will be inferred as "EmailAddress" Elasticsearch document field name.
      /// </example>
      public static Func<string, string>? CustomElasticsearchNamingAction
      {
         get => _customElasticsearchNamingAction.Value;
         set => _customElasticsearchNamingAction.Value = value;
      }

      /// <summary>
      /// You can extend the gridify supported operators by adding
      /// your own operators to OperatorManager.
      /// Custom operators must implement the IGridifyOperator interface
      /// and must start with '#' character.
      /// </summary>
      public static OperatorManager CustomOperators
      {
         get
         {
            // Return thread-local instance if set, otherwise return global instance
            return _customOperators.Value ?? _globalCustomOperators.Value;
         }
      }

      /// <summary>
      /// Sets a thread-local custom operator manager.
      /// This allows different threads or async contexts to have their own custom operators.
      /// </summary>
      /// <param name="operatorManager">The custom operator manager to use in this context</param>
      public static void SetCustomOperators(OperatorManager operatorManager)
      {
         _customOperators.Value = operatorManager;
      }

      /// <summary>
      /// Clears the thread-local custom operators, reverting to the global default.
      /// </summary>
      public static void ClearCustomOperators()
      {
         _customOperators.Value = null;
      }

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

      /// <summary>
      /// Resets all thread-local configuration values to their defaults.
      /// Useful for ensuring clean state between tests.
      /// </summary>
      public static void ResetToDefaults()
      {
         _entityFrameworkCompatibilityLayer.Value = null;
         _defaultPageSize.Value = null;
         _caseSensitiveMapper.Value = null;
         _allowNullSearch.Value = null;
         _ignoreNotMappedFields.Value = null;
         _avoidNullReference.Value = null;
         _disableNullChecks.Value = null;
         _caseInsensitiveFiltering.Value = null;
         _defaultDateTimeKind.Value = null;
         _customElasticsearchNamingAction.Value = null;
         _customOperators.Value = null;
      }
   }
}
