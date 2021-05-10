using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
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
      public Startup(IConfiguration configuration)
      {
         Configuration = configuration;
      }

      public IConfiguration Configuration { get; }
      private readonly string MyAllowSpecificOrigins = "_myAllowSpecificOrigins";

      // This method gets called by the runtime. Use this method to add services to the container.
      public void ConfigureServices(IServiceCollection services)
      {
         services.AddControllersWithViews();
         services.AddDbContext<AppDbContext>(opt => opt.UseInMemoryDatabase("Mock"));
         services.AddAutoMapper(typeof(Startup));
         services.AddSwaggerGen(c =>
         {
            // Set the comments path for the Swagger JSON and UI.
            var xmlPath = Path.Combine(AppContext.BaseDirectory, "SampleProject.xml");
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
      }

      // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
      public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
      {
         if (env.IsDevelopment())
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
         app.UseStaticFiles();

         app.UseRouting();

         app.UseCors(MyAllowSpecificOrigins);

         app.UseAuthorization();

         app.UseEndpoints(endpoints =>
         {
            endpoints.MapControllers();
         });
      }
   }
}
