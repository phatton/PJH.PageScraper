namespace PJH.PageScraper.Core.Interfaces;

public interface IScrapingClient
{
    Task<string> FetchHtmlAsync(Uri uri);
}
