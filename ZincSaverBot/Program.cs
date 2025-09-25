using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using ZincSaverBot.BotFunctionality.Interfaces;
using ZincSaverBot.Services;
using ZincSaverBot.Services.Interfaces;
using ZincSaverBot.Settings;

namespace ZincSaverBot;

internal class Program
{
    private static async Task Main(string[] args)
    {
        using var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices(async void (hostContext, services) =>
            {
                await ConfigureServices(services, hostContext.Configuration);
            })
            .Build();
        var logger = host.Services.GetRequiredService<ILogger<Program>>();

        var lifetime = host.Services.GetRequiredService<IHostApplicationLifetime>();
        lifetime.ApplicationStarted.Register(() => logger.LogInformation("Application started"));
        lifetime.ApplicationStopping.Register(() => logger.LogInformation("Application stopping"));
        lifetime.ApplicationStopped.Register(() => logger.LogInformation("Application stopped"));

        await host.StartAsync();

        await host.WaitForShutdownAsync();
    }

    private static async Task ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        services.AddLogging(builder =>
        {
            builder.AddConsole();

            var sentrySettings = configuration.GetRequiredSection("Sentry").Get<SentrySettings>();
            services.Configure<SentrySettings>(configuration.GetSection("Sentry"));
            builder.AddSentry(options =>
            {
                options.Dsn = sentrySettings!.Dsn;
                options.Environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT");

                options.MinimumBreadcrumbLevel = sentrySettings.MinimumBreadcrumbLevel;
                options.MaxBreadcrumbs = sentrySettings.MaxBreadcrumbs;

                options.MinimumEventLevel = sentrySettings.MinimumEventLevel;
            });
        });

        #region Bot

        services.AddSingleton<IBotInfo, BotInfo>();

        var botSettings = configuration.GetRequiredSection("Bot").Get<BotSettings>();
        services.AddSingleton<ITelegramBotClient>(new TelegramBotClient(botSettings?.Token ?? ""));
        services.AddHostedService<BotService>();

        await SetupTypes(services);

        services.AddScoped<ICommandRegistryService, CommandRegistryService>();
        services.AddScoped<ICallbackRegistryService, CallbackRegistryService>();
        services.AddScoped<CobaltServices>();
        services.AddScoped<Cobalt>();

        #endregion
    }

    private static Task SetupTypes(IServiceCollection services)
    {
        var commands = typeof(Program).Assembly.GetTypes()
            .Where(t =>
                typeof(ICommand).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });
        foreach (var type in commands)
        {
            services.AddTransient(typeof(ICommand), type);
        }

        var callbacks = typeof(Program).Assembly.GetTypes()
            .Where(t =>
                typeof(ICallback).IsAssignableFrom(t) && t is { IsInterface: false, IsAbstract: false });
        foreach (var type in callbacks)
        {
            services.AddTransient(typeof(ICallback), type);
        }

        return Task.CompletedTask;
    }
}