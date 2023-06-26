using Microsoft.Extensions.Logging;
using PJH.PageScraper.Core.Interfaces;
using Knyaz.Optimus;
using Knyaz.Optimus.ScriptExecuting.Jint;

namespace PJH.PageScraper.Infrastructure.Clients;

public class BrowserScrapingClient : IScrapingClient
{
    private readonly Engine engine;
    private readonly ILogger logger;
    private string url = string.Empty;

    public BrowserScrapingClient(ILogger<BrowserScrapingClient> logger)
    {
        this.logger = logger;

        this.engine = EngineBuilder.New()
				.ConfigureResourceProvider(res => res.Http(h => h.ConfigureClientHandler(c =>
					//avoid "System.Net.Http.HttpRequestException: The SSL connection could not be established,..."
					c.ServerCertificateCustomValidationCallback = (message, certificate2, arg3, arg4) => true))
                    .Notify(
                        req => { 
                            req.Headers["Accept"] = "*/*";
                            req.Headers["User-Agent"] = "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.60 Safari/537.36 Edg/100.0.1185.29";
                        },
                        resp => {
                        
                        }))
				.UseJint()
				//.UseJurassic()
				//redirect browser's console to the application console.
				//.Window(w => w.SetConsole(new SystemConsole()))
				.Build();
    }

    public async Task<string> FetchHtmlAsync(Uri uri)
    {
        this.url = uri.ToString();

        this.logger.LogInformation(new EventId(),"Loading Url: " + this.url);
        var page = await engine.OpenUrl(url);
        this.logger.LogInformation(new EventId(), page is HttpPage httpPage ? httpPage.HttpStatusCode.ToString() : "Error");

        return page.Document.OuterHTML;
    }
}
