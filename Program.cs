using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using BudgetReview.Gathering;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using Serilog.Events;

namespace BudgetReview
{
    class Program
    {
        static async Task Main(string[] args)
        {
            // Hello! This Program class mostly deals with the business of being a program.
            // The starting point for the program's useful logic is in BudgetReview.Execute.

            Env.Load();
            ConfigureLogging();

            Console.WriteLine("# Budget Review");

            EmitDebugInfo();

            var programHost = new HostBuilder()
                .ConfigureServices(ConfigureServices)
                .UseConsoleLifetime()
                .Build();

            try
            {
                var clock = new Stopwatch();
                clock.Start();

                var budgetReview = programHost.Services.GetRequiredService<BudgetReview>();
                await budgetReview.Execute();

                Log.Debug("Budget review took {Duration} to run", clock.Elapsed);
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        private static void ConfigureServices(HostBuilderContext _, IServiceCollection services)
        {
            services.AddSingleton<BudgetReview>();
            services.AddTransient<IGatheringRoot, GatheringRoot>();
        }

        private static void EmitDebugInfo()
        {
            Log.Debug($"Current culture: {CultureInfo.CurrentCulture}");
            Log.Debug($"Current UI culture: {CultureInfo.CurrentUICulture}");
            Log.Debug("Current directory: {CurrentDirectory}", Directory.GetCurrentDirectory());
            Log.Debug("Env file parsed: {IsEnvParsed}", Env.Get("env_file_parsed", "No"));
            Log.Debug("Command line args: {Args}", Environment.GetCommandLineArgs());
        }

        private static void ConfigureLogging()
        {
            var configuredMin = Env.Get("min_log_level", "Warning");
            var loggerConfig = new LoggerConfiguration()
                .MinimumLevel.Is(Enum.Parse<LogEventLevel>(configuredMin))
                .WriteTo.Console(outputTemplate: "[{Level:u4}] {Message:lj}{NewLine}{Exception}");

            Log.Logger = loggerConfig.CreateLogger();
        }
    }
}
