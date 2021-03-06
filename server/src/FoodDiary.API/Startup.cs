using System.IO;
using System.Reflection;
using AutoMapper;
using FoodDiary.API.Extensions;
using FoodDiary.API.Middlewares;
using FoodDiary.API.Options;
using FoodDiary.Application.Extensions;
using FoodDiary.Import.Extensions;
using FoodDiary.Infrastructure;
using FoodDiary.PdfGenerator.Extensions;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;

namespace FoodDiary.API
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            services.Configure<ImportOptions>(Configuration.GetSection("Import"));

            services.AddDbContext<FoodDiaryContext>();

            services.AddRepositories();
            services.AddUtils();
            services.AddPagesPdfGenerator();
            services.AddPagesJsonImportServices();
            services.AddApplicationDependencies();

            services.AddAutoMapper(Assembly.GetExecutingAssembly());

            var clientAppUrl = Configuration.GetValue<string>("ClientAppUrl");
            services.AddCors(options => options.AddPolicy("AllowClientApp", builder =>
                 builder.WithOrigins(clientAppUrl)
                     .AllowAnyHeader()
                     .AllowAnyMethod()));

            services.AddControllers();
            services.AddFoodDiarySwagger();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.MigrateDatabase();

            app.UseCors("AllowClientApp");

            app.UseRouting();

            app.UseMiddleware<ExceptionHandlerMiddleware>();

            app.UseFoodDiarySwagger();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });

            app.UseStaticFiles(new StaticFileOptions()
            {
                FileProvider = new PhysicalFileProvider(Path.Combine(Directory.GetCurrentDirectory(), @"Resources")),
                RequestPath = new PathString("/Resources")
            });
        }
    }
}
