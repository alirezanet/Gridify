using System;
using System.Runtime;

namespace Gridify
{
   //This original fork from https://github.com/zzzprojects/System.Linq.Dynamic.Core
   internal static class RuntimeInformationUtils
    {
        public static bool IsBlazorWASM;

        static RuntimeInformationUtils()
        {
            IsBlazorWASM =
                // Used for Blazor WebAssembly .NET Core 3.x / .NET Standard 2.x
                Type.GetType("Mono.Runtime") != null ||

               // Use for Blazor WebAssembly .NET
               // See also https://github.com/mono/mono/pull/19568/files
               System.Runtime.InteropServices.RuntimeInformation.IsOSPlatform(System.Runtime.InteropServices.OSPlatform.Create("BROWSER"));
        }
    }
}
