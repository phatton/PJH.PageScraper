// See https://aka.ms/new-console-template for more information
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using PJH.PageScraper.Core.Interfaces;
using PJH.PageScraper.Infrastructure.Clients;
using PJH.PageScraper.Infrastructure.Services;

internal class Program
{
    public  static void Main(string[] args)
    {
        CreateHostBuilder(args).Build().Run();        
    }
    public static IHostBuilder CreateHostBuilder(string[] args) =>
                Host.CreateDefaultBuilder(args)
                    .ConfigureServices((hostContext, services) =>
                    {
                        // Add services to the container.
                        services.AddHttpClient();
                        // services.AddTransient<IScrapingClient, HttpScrapingClient>();
                        services.AddTransient<IScrapingClient, BrowserScrapingClient>();
                        services.AddTransient<IScrapingService, ScrapingService>();
                        services.AddHostedService<Worker>();
                    });
}

internal class Worker : IHostedService
{
    private readonly ILogger logger;
    private readonly IScrapingService scraptingService;

    public Worker(
        ILogger<Worker> logger,
        IHostApplicationLifetime appLifetime,
        IScrapingService scraptingService)
    {
        this.logger = logger;
        this.scraptingService = scraptingService;

        appLifetime.ApplicationStarted.Register(OnStarted);
        appLifetime.ApplicationStopping.Register(OnStopping);
        appLifetime.ApplicationStopped.Register(OnStopped);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("1. StartAsync has been called.");

         string url = "https://www.xcentium.com/";//"https://en.wikipedia.org/wiki/List_of_programmers"
         var pageData = await this.scraptingService.GetPageData(url);

        foreach(var image in pageData.ImageList)
        {
            logger.LogInformation(image.Url);
        }

        logger.LogInformation("Page Word Count: " + pageData.WordCount);

        foreach(var word in pageData.WordOccurences)
        {
            logger.LogInformation("Word: \"" + word.Value + "\" Count: " + word.Count);
        }

        return;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        logger.LogInformation("4. StopAsync has been called.");

        return Task.CompletedTask;
    }

    private void OnStarted()
    {
        logger.LogInformation("2. OnStarted has been called.");
    }

    private void OnStopping()
    {
        logger.LogInformation("3. OnStopping has been called.");
    }

    private void OnStopped()
    {
        logger.LogInformation("5. OnStopped has been called.");
    }
}