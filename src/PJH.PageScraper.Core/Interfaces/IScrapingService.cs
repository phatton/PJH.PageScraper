using PJH.PageScraper.Core.Models;

namespace PJH.PageScraper.Core.Interfaces;

public interface IScrapingService
{
    Task<PageData> GetPageData(string url);
}
