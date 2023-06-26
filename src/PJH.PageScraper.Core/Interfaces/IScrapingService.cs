using PJH.PageScraper.Core.Models;

namespace PJH.PageScraper.Core.Interfaces;

public interface IScrapingService
{
    Task<PageData> GetPageDataAsync(string pageUrl);
    Task<PageData> GetPageDataAsync(Uri pageUrl);
}
