namespace PJH.PageScraper.Core.Models;

public class PageData
{
    public List<Image> ImageList { get; set; } = new List<Image>();

    public int WordCount { get; set; }

    public List<WordOccurance> WordOccurences { get; set; } = new List<WordOccurance>();
}
