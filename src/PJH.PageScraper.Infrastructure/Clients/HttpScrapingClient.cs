using Microsoft.Extensions.Logging;
using PJH.PageScraper.Core.Interfaces;

namespace PJH.PageScraper.Infrastructure.Clients;

public class HttpScrapingClient : IScrapingClient
{
    private readonly IHttpClientFactory httpClientFactory;   
    private readonly ILogger logger;

    public HttpScrapingClient(IHttpClientFactory httpClientFactory, ILogger<HttpScrapingClient> logger)
    {
        this.httpClientFactory = httpClientFactory;
        this.logger = logger;
    }

    public async Task<string> FetchHtml(Uri uri)
    {
        // Create the client with the factory
        HttpClient client = this.httpClientFactory.CreateClient();
        client.DefaultRequestHeaders.TryAddWithoutValidation("Accept", "*/*");
        client.DefaultRequestHeaders.TryAddWithoutValidation("User-Agent", "Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/100.0.4896.60 Safari/537.36 Edg/100.0.1185.29");

        string response = string.Empty;
        try
        {
            response = await client.GetStringAsync(uri);
        }
        catch(Exception ex)
        {
            this.logger.LogError(ex.Message);
        }
        return response;
    }
}
