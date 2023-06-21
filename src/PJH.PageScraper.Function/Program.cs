using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;
using PJH.PageScraper.Core.Interfaces;
using PJH.PageScraper.Infrastructure.Clients;
using PJH.PageScraper.Infrastructure.Services;

var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices(s =>
    {
        // s.AddTransient<IScrapingClient, HttpScrapingClient>();//Uncomment to use .NET HttpClient
        s.AddTransient<IScrapingClient, BrowserScrapingClient>();//Comment for use of .NET HttpClient
        //s.AddTransient<IScrapingClient, SeleniumWebDriverClient>();//Uncomment to use Selenium WebDriver
        s.AddTransient<IScrapingService, ScrapingService>();

        // s.Configure<LoggerFilterOptions>(options =>
        // {
        //     // The Application Insights SDK adds a default logging filter that instructs ILogger to capture only Warning and more severe logs. Application Insights requires an explicit override.
        //     // Log levels can also be configured using appsettings.json. For more information, see https://learn.microsoft.com/en-us/azure/azure-monitor/app/worker-service#ilogger-logs
        //     LoggerFilterRule toRemove = options.Rules.FirstOrDefault(rule => rule.ProviderName
        //         == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");

        //     if (toRemove is not null)
        //     {
        //         options.Rules.Remove(toRemove);
        //     }
        // });


        //Add HttpClientFactory
        s.AddHttpClient();        
    })
    
    .Build();

await host.RunAsync();
