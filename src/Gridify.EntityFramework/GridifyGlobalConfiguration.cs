namespace Gridify.EntityFramework
{
   public static partial class GridifyGlobalConfiguration
   {
      public static void EnableEntityFrameworkCompatibilityLayer()
      {
         Gridify.GridifyGlobalConfiguration.EntityFrameworkCompatibilityLayer = true;
      }

      public static void DisableEntityFrameworkCompatibilityLayer()
      {
         Gridify.GridifyGlobalConfiguration.EntityFrameworkCompatibilityLayer = false;
      }
      
   }
}