using System.Runtime.CompilerServices;

namespace Gridify
{
   public static partial class GridifyGlobalConfiguration
   {
      public static bool EntityFrameworkCompatibilityLayer
      {
         get => GridifyExtensions.EntityFrameworkCompatibilityLayer;
         set => GridifyExtensions.EntityFrameworkCompatibilityLayer = value;
      }

      public static int DefaultPageSize
      {
         get => GridifyExtensions.DefaultPageSize;
         set => GridifyExtensions.DefaultPageSize = value;
      }

      public static void EnableEntityFrameworkCompatibilityLayer()
      {
         GridifyExtensions.EntityFrameworkCompatibilityLayer = true;
      }

      public static void DisableEntityFrameworkCompatibilityLayer()
      {
         GridifyExtensions.EntityFrameworkCompatibilityLayer = false;
      }
   }
}