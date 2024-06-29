using System;
using System.Linq;
using System.Reflection;
using Microsoft.Extensions.DependencyInjection;

namespace Gridify;

/// <summary>
/// Extension methods for configuring Dependency Injection related to GridifyMapper.
/// </summary>
public static class DependencyInjectionExtensions
{
   private static readonly (Type BaseType, Type Interface) GridifyMapperType = (typeof(GridifyMapper<>), typeof(IGridifyMapper<>));
   private static readonly (Type BaseType, Type Interface) QueryBuilderType = (typeof(QueryBuilder<>), typeof(IQueryBuilder<>));

   /// <summary>
   /// Automatically scans an assembly for classes that inherit from GridifyMapper&lt;T&gt; and registers them in the Dependency Injection container.
   /// </summary>
   /// <param name="services">The IServiceCollection to which the mappers should be added.</param>
   /// <param name="assembly">The assembly to scan for GridifyMapper implementations.</param>
   /// <param name="lifetime">The service lifetime for the registered mappers (default is Singleton).</param>
   public static IServiceCollection AddGridifyMappers(this IServiceCollection services, Assembly assembly,
       ServiceLifetime lifetime = ServiceLifetime.Singleton)
   {
      return ScanAndAddByType(services, assembly, lifetime, GridifyMapperType);
   }

   /// <summary>
   /// Automatically scans an assembly for classes that inherit from QueryBuilder&lt;T&gt; and registers them in the Dependency Injection container.
   /// </summary>
   /// <param name="services">The IServiceCollection to which the query builders should be added.</param>
   /// <param name="assembly">The assembly to scan for QueryBuilder implementations.</param>
   /// <param name="lifetime">The service lifetime for the registered query builders (default is Singleton).</param>
   public static IServiceCollection AddGridifyQueryBuilders(this IServiceCollection services, Assembly assembly,
      ServiceLifetime lifetime = ServiceLifetime.Singleton)
   {
      return ScanAndAddByType(services, assembly, lifetime, QueryBuilderType);
   }

   /// <summary>
   /// Automatically scans an assembly for classes that inherit from either GridifyMapper&lt;T&gt; or QueryBuilder&lt;T&gt;
   /// and registers them in the Dependency Injection container.
   /// </summary>
   /// <param name="services">The IServiceCollection to which the mappers and query builders should be added.</param>
   /// <param name="assembly">The assembly to scan for GridifyMapper and QueryBuilder implementations.</param>
   /// <param name="lifetime">The service lifetime for the registered types (default is Singleton).</param>
   public static IServiceCollection AddGridify(this IServiceCollection services, Assembly assembly,
      ServiceLifetime lifetime = ServiceLifetime.Singleton)
   {
      var types = assembly.GetTypes()
         .Where(type => type is
         { IsAbstract: false, IsGenericTypeDefinition: false, BaseType.IsGenericType: true, BaseType.Namespace: nameof(Gridify) })
         .Select(type => (type, typeDefinition: type.BaseType?.GetGenericTypeDefinition()))
         .Where(q => q.typeDefinition != null && q.typeDefinition == QueryBuilderType.BaseType || q.typeDefinition == GridifyMapperType.BaseType);

      foreach (var (type, typeDefinition) in types)
      {
         var genericArguments = type.BaseType?.GetGenericArguments();
         if (genericArguments is not { Length: 1 }) continue;
         var targetType = genericArguments[0];
         var interfaceType = typeDefinition == GridifyMapperType.BaseType
            ? GridifyMapperType.Interface.MakeGenericType(targetType)
            : QueryBuilderType.Interface.MakeGenericType(targetType);
         services.Add(new ServiceDescriptor(interfaceType, type, lifetime));
      }

      return services;
   }

   private static IServiceCollection ScanAndAddByType(IServiceCollection services, Assembly assembly, ServiceLifetime lifetime,
      (Type BaseType, Type Interface) scanType)
   {
      var targetTypes = assembly.GetTypes()
         .Where(type =>
            type is { IsAbstract: false, IsGenericTypeDefinition: false, BaseType.IsGenericType: true, BaseType.Namespace: nameof(Gridify) } &&
            type.BaseType.GetGenericTypeDefinition() == scanType.BaseType);

      foreach (var type in targetTypes)
      {
         var genericArguments = type.BaseType?.GetGenericArguments();
         if (genericArguments is not { Length: 1 }) continue;
         var targetType = genericArguments[0];
         var interfaceType = scanType.Interface.MakeGenericType(targetType);
         services.Add(new ServiceDescriptor(interfaceType, type, lifetime));
      }

      return services;
   }
}
