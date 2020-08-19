using System;
using System.Runtime.InteropServices;
using IISLogDeleter.Models;
using IISLogDeleter.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.EventLog;

namespace IISLogDeleter
{
    class Program
    {
        static void Main(string[] args)
        {
            CreateHost(args).Build().Run();
        }

        static IHostBuilder CreateHost(string[] args) =>
            new HostBuilder()
                .ConfigureAppConfiguration((ctx, cfg) =>
                {
                    cfg.AddJsonFile("appsettings.json", true, true);
                    cfg.AddEnvironmentVariables("IISLogDeleter_");
                    cfg.AddCommandLine(args);
                })
                .ConfigureLogging((ctx, log) =>
                {
                    var cfg = ctx.Configuration;
                    log.SetMinimumLevel(LogLevel.Information);
                    log.AddConfiguration(cfg.GetSection("Logging"));
                    log.AddConsole();
                    log.AddEventLog();
                })
                .ConfigureServices((ctx, srv) =>
                {
                    var cfg = ctx.Configuration;

                    srv.AddOptions<AppSettings>()
                        .Bind(cfg, o => o.BindNonPublicProperties = true)
                        .Validate(s => s.DeletionInterval != default,
                            $"{nameof(AppSettings.DeletionInterval)} must be specified.")
                        .Validate(s => s.LogsFolder != default,
                            $"{nameof(AppSettings.LogsFolder)} must be specified.")
                        .Validate(s => s.DeleteFilesOlderThan != default,
                            $"{nameof(AppSettings.DeleteFilesOlderThan)} must be specified.");

                    srv.AddHostedService<LogDeleter>();
                })
                .UseWindowsService();
    }
}