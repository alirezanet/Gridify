using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SampleProject.Helpers;

namespace SampleProject
{
   public class Program
   {
      public static void Main(string[] args)
      {
         var host = CreateHostBuilder(args).Build();
         using (var scope = host.Services.CreateScope())
         {
            var services = scope.ServiceProvider;
            var context = services.GetRequiredService<AppDbContext>();
            SeedData.Initialize(services);
         }
         host.Run();
      }

      public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                   webBuilder.UseStartup<Startup>();
                });
   }
}
