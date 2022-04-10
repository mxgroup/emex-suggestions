using System;
using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Reflection;
using MediatR;
using MediatR.Extensions.FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using Suggestions.Common.Options;
using Suggestions.Infrastructure;
using Suggestions.Logic;
using Suggestions.RestApi.Auth;
using Suggestions.RestApi.Extensions;
using Suggestions.RestApi.Extensions.Filters;
using Swashbuckle.AspNetCore.Filters;

namespace Suggestions.RestApi
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostEnvironment env)
        {
            Configuration = configuration;
            Environment = env;
        }

        public IConfiguration Configuration { get; }
        public IHostEnvironment Environment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Настройки политик CORS, если потребуются разные
            services.AddCors(options =>
            {
                options.AddDefaultPolicy(builder => builder
                    .SetIsOriginAllowedToAllowWildcardSubdomains()
                    .WithOrigins(
                        "http://localhost:61076",
                        "http://localhost:57359",
                        "http://localhost:3000",
                        "https://emex.test",
                        "https://*.emex.test",
                        "https://emex.ru",
                        "https://*.emex.ru",
                        "https://emex.csssr.ru",
                        "https://*.csssr.ru",
                        "https://emex.csssr.cloud",
                        "https://*.emex.csssr.cloud",
                        "https://emex-header.csssr.cloud",
                        "https://*.emex-header.csssr.cloud")
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .AllowCredentials()
                );
            });

            services.AddControllers(options =>
            {
                options.Filters.Add(typeof(BadOperationExceptionFilter));
                options.Filters.Add(typeof(FluentValidationExceptionFilter));
            });

            services.AddResponseCompression(options => { options.Providers.Add<GzipCompressionProvider>(); });

            services.AddHealthChecks()
                .AddCheck("ConsulConfigLoadCheck", () =>
                {
                    if (Configuration.GetValue("ConsulConfig", false))
                    {
                        return HealthCheckResult.Healthy();
                    }

                    return HealthCheckResult.Unhealthy(
                        "Отсутствует признак IsConsulConfig, что может свидетельствовать об ошибках получения настроек из Consul, проверьте наличие в логе сообщений 'Consul OnLoadException'");
                });

            // Настройки
            services.AddOptions<AbcpOptions>().Bind(Configuration.GetSection("ABCP")).ValidateDataAnnotations();
            services.AddOptions<SearchHistoryOptions>().Bind(Configuration.GetSection("SearchHistory")).ValidateDataAnnotations();
            services.AddOptions<IntegrationApiOptions>().Bind(Configuration.GetSection("IntegrationApi")).ValidateDataAnnotations();

            services.TryAddSingleton<IHttpContextAccessor, HttpContextAccessor>();
            services.AddMemoryCache();

            // MediatR
            services.AddMediatR(typeof(LogicModule).Assembly);
            services.AddFluentValidation(new[] { typeof(LogicModule).Assembly });

            // Swagger
            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo
                {
                    Title = "API",
                    Version = "v1",
                    Description = "API сервиса подсказок"
                });

                c.EnableAnnotations();
                c.ExampleFilters();
                string xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
                string xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
                c.IncludeXmlComments(xmlPath);
            });
            services.AddSwaggerExamplesFromAssemblyOf<Startup>();

            // Модули
            LogicModule.ConfigureServices(services, Configuration);
            InfrastructureModule.ConfigureServices(services, Configuration);

            // Авторизация
            services.AddScoped<IAuthLogic, AuthLogic>();

            // Http-клиенты
            services
                .AddHttpClient<OldWebsiteHttpClient>();
            services.AddHttpClient<HttpClientWrapper>()
                .ConfigurePrimaryHttpMessageHandler(() =>
                {
                    // Отключаем автоматическую обработку кук для совместно используемых HttpMessageHandler
                    return new SocketsHttpHandler
                    {
                        UseCookies = false
                    };
                });

            // Jaeger
            services.AddJaeger(Configuration);
            services.AddOpenTracing();
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseCors();

            // Elastic Apm
            app.UseApm(Configuration);

            // Swagger
            app.UseSwagger(c =>
            {
                c.RouteTemplate = "suggestions/swagger/{documentName}/swagger.json";
                //c.PreSerializeFilters.Add((swaggerDoc, httpReq) =>
                //{
                //    // Чтобы при использовании путей в nginx, api не вызывались относительно хоста
                //    string referer = httpReq.Headers["Referer"].ToString();
                //    var address = referer.Replace("index.html", "");
                //    swaggerDoc.Servers = new List<OpenApiServer> { new OpenApiServer
                //        { Url = address } };
                //});
            });

            app.UseSwaggerUI(options =>
            {
                options.SwaggerEndpoint("swagger/v1/swagger.json", "Suggestions");
                options.RoutePrefix = "suggestions";
            });

            app.UseRouting();

            app.UseAuthorization();
            app.UseMiddleware<VisitorTrackingMiddleware>();

            app.UseEndpoints(endpoints => 
            { 
                endpoints.MapControllers(); 
                endpoints.MapHealthChecks("/health");
                if (!env.IsProduction())
                {
                    endpoints.MapGet("/debug-config", ctx =>
                    {
                        var config = (Configuration as IConfigurationRoot).GetDebugView();
                        return ctx.Response.WriteAsync(config);
                    });
                }
            });
        }
    }
}