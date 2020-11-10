using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;
using System.IO;

namespace Microsoft.AspNetCore.Hosting
{
    public static class WebHostEnvironmentExtensions
    {
        public static Serilog.Core.Logger ConfigureSerilog(this IWebHostEnvironment env)
        {
            string directory;
            if (env.IsDevelopment())
            {
                directory = $"{env.ContentRootPath}\\Tracing\\";
            }
            else
            {
                directory = $"{env.ContentRootPath}\\..\\Tracing\\";
            }

            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            var config = new LoggerConfiguration()
                .WriteTo
                .File($"{directory}{{Date}}.log", 
                outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz}\t[{Level:w3}]\t{Message}\t[{SourceContext}]{NewLine}{Exception}",
                rollingInterval: RollingInterval.Day,
                fileSizeLimitBytes: 10_000_000,
                rollOnFileSizeLimit: true                
                );

            config.MinimumLevel.Is(LogEventLevel.Debug);

            return config.CreateLogger();
        }
    }
}
