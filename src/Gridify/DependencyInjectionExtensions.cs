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
   private static readonly Type GridifyMapperType = typeof(GridifyMapper<>);
   private static readonly Type QueryBuilderType = typeof(QueryBuilder<>);

   /// <summary>
   /// Automatically scans an assembly for classes that inherit from GridifyMapper&lt;T&gt; and registers them in the Dependency Injection container.
   /// </summary>
   /// <param name="services">The IServiceCollection to which the mappers should be added.</param>
   /// <param name="assembly">The assembly to scan for GridifyMapper implementations.</param>
   /// <param name="lifetime">The service lifetime for the registered mappers (default is Singleton).</param>
   public static IServiceCollection AddGridifyMappers(this IServiceCollection services, Assembly assembly,
       ServiceLifetime lifetime = ServiceLifetime.Singleton)
   {
      var mapperTypes = assembly.GetTypes()
          .Where(type =>
              type is { IsAbstract: false, IsGenericTypeDefinition: false, BaseType.IsGenericType: true } &&
              type.BaseType.GetGenericTypeDefinition() == GridifyMapperType);

      foreach (var mapper in mapperTypes)
      {
         var genericArguments = mapper.BaseType?.GetGenericArguments();
         if (genericArguments is not { Length: 1 }) continue;
         var targetType = genericArguments[0];
         var interfaceType = GridifyMapperType.MakeGenericType(targetType);
         services.Add(new ServiceDescriptor(interfaceType, mapper, lifetime));
      }

      return services;
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
      var queryBuilderTypes = assembly.GetTypes()
         .Where(type =>
            type is { IsAbstract: false, IsGenericTypeDefinition: false, BaseType.IsGenericType: true } &&
            type.BaseType.GetGenericTypeDefinition() == QueryBuilderType);

      foreach (var queryBuilder in queryBuilderTypes)
      {
         var genericArguments = queryBuilder.BaseType?.GetGenericArguments();
         if (genericArguments is not { Length: 1 }) continue;
         var targetType = genericArguments[0];
         var interfaceType = QueryBuilderType.MakeGenericType(targetType);
         services.Add(new ServiceDescriptor(interfaceType, queryBuilder, lifetime));
      }

      return services;
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
         .Where(type =>
            type is { IsAbstract: false, IsGenericTypeDefinition: false, BaseType.IsGenericType: true } &&
            (type.BaseType.GetGenericTypeDefinition() == QueryBuilderType || type.BaseType.GetGenericTypeDefinition() == GridifyMapperType));

      foreach (var type in types)
      {
         var genericArguments = type.BaseType?.GetGenericArguments();
         if (genericArguments is not { Length: 1 }) continue;
         var targetType = genericArguments[0];
         var interfaceType = type.MakeGenericType(targetType);
         services.Add(new ServiceDescriptor(interfaceType, interfaceType, lifetime));
      }

      return services;
   }
}
