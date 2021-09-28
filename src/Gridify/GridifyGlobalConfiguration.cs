using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Gridify.EntityFramework")]
namespace Gridify
{
   public static partial class GridifyGlobalConfiguration
   {
      internal static bool EntityFrameworkCompatibilityLayer { get; set; }
      public static int DefaultPageSize { get; set; } = 20;
   }
}