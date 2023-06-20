namespace PJH.PageScraper.Core.Interfaces;

public interface IScrapingClient
{
    Task<string> FetchHtml(Uri uri);
}
