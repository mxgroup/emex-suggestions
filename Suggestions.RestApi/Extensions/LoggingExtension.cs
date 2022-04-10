using System.Collections.Generic;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Serilog;
using Serilog.Configuration;
using Serilog.Events;
using Serilog.Formatting.Json;

namespace Suggestions.RestApi.Extensions
{
    public static class LoggingExtension
    {
        private const string OutputTemplate =
            "{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{ThreadId}] [{Level:u3}] [{RequestId}] {SourceContext} - {Message:lj}{NewLine}{Exception}";
        public static IWebHostBuilder ConfigureSerilog(this IWebHostBuilder builder)
        {
            return builder.UseSerilog((context, configuration) => configuration.ConfigureDefaults(context));
        }

        private static LoggerConfiguration ConfigureDefaults(this LoggerConfiguration config, WebHostBuilderContext context)
        {
            return config.ReadFrom.Configuration(context.Configuration)
                .Enrich.WithProperty("ServiceName", context.Configuration.GetValue<string>("ServiceName"))
                .Enrich.WithProperty("Configuration", context.HostingEnvironment.EnvironmentName)
                .Enrich.WithThreadId()
                .Enrich.FromLogContext()
                .Filter.ByExcluding(ExcludeMessage)
                .WriteTo.Console(new JsonFormatter(renderMessage: true))
                .ConfigureFileLogging(context.Configuration);
        }

        private static readonly HashSet<string> ExcludedPaths = new HashSet<string>()
        {
            "/health"
        };

        private static bool ExcludeMessage(LogEvent arg)
        {
            if (arg.Properties.ContainsKey("RequestPath"))
            {
                var path = arg.Properties["RequestPath"].ToString().Trim('"');
                if (ExcludedPaths.Contains(path))
                {
                    return true;
                }
            }

            return false;
        }

        private static LoggerConfiguration ConfigureFileLogging(this LoggerConfiguration config,
            IConfiguration configuration)
        {
            var errorFile = configuration.GetValue<string>("Serilog:ErrorsFilePath");
            var allFile = configuration.GetValue<string>("Serilog:AllFilePath");

            if (!string.IsNullOrEmpty(errorFile))
            {
                config = config.WriteTo.Async(a =>
                    a.Conditional(e => e.Level == LogEventLevel.Error || e.Level == LogEventLevel.Fatal,
                        b => b.ToFile(errorFile)));
            }

            if (!string.IsNullOrEmpty(allFile))
            {
                config = config.WriteTo.Async(x => x.ToFile(allFile));
            }

            return config;
        }

        private static LoggerConfiguration ToFile(this LoggerSinkConfiguration c, string filePath)
        {
            return c.File(filePath,
                outputTemplate: OutputTemplate,
                rollingInterval: RollingInterval.Day, fileSizeLimitBytes: 100 * 1024 * 1024, rollOnFileSizeLimit: true, retainedFileCountLimit: 14);
        }
    }
}