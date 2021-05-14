using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.SpaServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.IO;
using System.Reflection;

namespace SampleProject
{
   public class Startup
   {
      public Startup(IConfiguration configuration, IWebHostEnvironment env)
      {
         Configuration = configuration;
         Env = env;
      }

      public IConfiguration Configuration { get; }
      private readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
      private readonly IWebHostEnvironment Env;

      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddControllersWithViews();
         services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("Mock"));
         services.AddAutoMapper(typeof(Startup));
         services.AddSwaggerGen(c =>
         {
            // Set the comments path for the Swagger JSON and UI.
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);
         });

         services.AddCors(c =>
         {
            c.AddPolicy(name: MyAllowSpecificOrigins, opt =>
                                    opt.WithOrigins("http://localhost:5000",
                                                    "http://localhost:8080")
                                        .AllowAnyHeader()
                                        .AllowAnyMethod()
                                        .AllowCredentials());
         });

         services.AddSpaStaticFiles(configuration => configuration.RootPath = "wwwroot");
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app)
      {
         if (Env.IsDevelopment())
         {
            app.UseDeveloperExceptionPage();
         }
         else
         {
            app.UseExceptionHandler("/Home/Error");
            // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
            //app.UseHsts();
         }

         app.UseSwagger();
         app.UseSwaggerUI(c =>
         {
            c.SwaggerEndpoint("/swagger/v1/swagger.json", "Gridify SampleProject API");
         });

         //app.UseHttpsRedirection();
         app.UseSpaStaticFiles();

         app.UseStaticFiles();

         app.UseRouting();

         app.UseCors(MyAllowSpecificOrigins);

         app.UseAuthorization();

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllers();
         });

         app.UseSpa(spa =>
         {
            spa.Options.SourcePath = "wwwroot";
            if (Env.IsDevelopment())
            {
               spa.UseProxyToSpaDevelopmentServer("http://localhost:8080/");
            }
         });
      }
   }
}
