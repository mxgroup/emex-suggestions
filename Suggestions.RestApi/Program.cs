using System;
using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Suggestions.RestApi.Extensions;
using Winton.Extensions.Configuration.Consul;

namespace Suggestions.RestApi
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args)
        {
            return Host.CreateDefaultBuilder(args)
                .ConfigureAppConfiguration((context, builder) =>
                {
                    builder.AddJsonFile("appsettings.json", false);

                    var config = builder.Build();
                    var consulUrl = config["ConsulUrl"];
                    var consulContainerUrls = config.GetSection("ConsulContainerUrls").Get<IList<string>>();
                    var isIntegrationTesting = config["IsIntegrationTesting"] == "true";
                    var useConsul = !isIntegrationTesting && !string.IsNullOrEmpty(consulUrl) && !context.HostingEnvironment.IsDevelopment() && context.HostingEnvironment.EnvironmentName != "DevStaging";
                  
                    if (useConsul)
                    {
                        var urls = new List<Uri>() { new Uri(consulUrl) };
                        if (context.HostingEnvironment.IsProduction() && consulContainerUrls != null)
                        {
                            foreach (var url in consulContainerUrls)
                            {
                                urls.Add(new Uri(url));
                            }
                        }

                        foreach (var url in urls)
                        {
                            builder.AddConsul(
                                $"emex.ru-suggestions/{context.HostingEnvironment.EnvironmentName}",
                                options =>
                                {
                                    options.ConsulConfigurationOptions = configuration =>
                                    {
                                        configuration.Address = url;
                                    };

                                    options.ReloadOnChange = true;
                                    options.Optional = true;
                                    options.OnLoadException = exceptionContext =>
                                    {
                                        Console.WriteLine($"Consul OnLoadException (URL: {url}, Key: {exceptionContext.Source.Key}, Message: {exceptionContext.Exception?.Message})");
                                        exceptionContext.Ignore = true;
                                    };
                                });
                        }
                    }

                    builder.AddJsonFile($"appsettings.{context.HostingEnvironment.EnvironmentName}.json", true);
                    builder.AddJsonFile($"appsettings.machine.{Environment.MachineName}.json", true);
                    builder.AddEnvironmentVariables();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                        .ConfigureSerilog();
                });
        }
    }
}