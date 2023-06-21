using HtmlAgilityPack;
using Microsoft.Extensions.Logging;
using PJH.PageScraper.Core.Interfaces;
using PJH.PageScraper.Core.Models;
using System.Text.RegularExpressions;

namespace PJH.PageScraper.Infrastructure.Services;

public class ScrapingService : IScrapingService
{
    private readonly IScrapingClient scrapingClient;    
    private readonly ILogger logger;
    

    public ScrapingService(IScrapingClient scrapingClient, ILogger<ScrapingService> logger)
    {
        this.scrapingClient = scrapingClient;
        this.logger = logger;
    }

    public async Task<PageData> GetPageData(string pageUrl)
    {
        if(string.IsNullOrEmpty(pageUrl))
        {
            throw new ArgumentNullException(nameof(pageUrl));
        }
        
        Uri pageUri = new Uri(pageUrl);

        return await GetPageData(pageUri);
    }

    public async Task<PageData> GetPageData(Uri pageUrl)
    {
        if(pageUrl == null)
        {
            throw new ArgumentNullException(nameof(pageUrl));
        }

        PageData pageData = new PageData();

        //Retrieve HTML string
        var response = await this.scrapingClient.FetchHtml(pageUrl);
        
        //If HTML is available
        if(!string.IsNullOrEmpty(response))
        {
            try
            {
                //Load HTML to HtmlDocument
                HtmlDocument htmlDoc = new HtmlDocument();
                htmlDoc.LoadHtml(response);

                //Extract Images
                pageData.ImageList = ExtractPageImages(htmlDoc, pageUrl);

                //Extract Page word list
                List<string> words = ExtractPageWords(htmlDoc);

                //Set WordCount
                pageData.WordCount = words.Count();

                //Set WordOccurences
                pageData.WordOccurences = CalculateWordOccurences(words, 10);        
                
            }
            catch(Exception ex)
            {
                //Log
                this.logger.LogError(ex.Message);
            }
        }

        return pageData;
    }

    private List<Image> ExtractPageImages(HtmlDocument htmlDoc, Uri pageUrl)
    {
        List<Image> images = new List<Image>();

        var pageImages = htmlDoc.DocumentNode.Descendants("img").ToList();

        foreach (var pageImage in pageImages)
        {
            if (pageImage.HasAttributes)
            {
                HtmlAttribute imageSourceAttribute = pageImage.Attributes["src"];
                HtmlAttribute imageAltAttribute = pageImage.Attributes["alt"];
                HtmlAttribute imageWidthAttribute = pageImage.Attributes["width"];
                HtmlAttribute imageHeightAttribute = pageImage.Attributes["Height"];

                Image image = new Image();
                image.Url = imageSourceAttribute?.Value ?? string.Empty;

                //Check that the image has a valid Source.
                if (!string.IsNullOrEmpty(image.Url))
                {
                    //Check that the image was not already added to the list
                    if (images.FirstOrDefault(i => i.Url == image.Url) is null)
                    {
                        //Make sure the image url is fully qualified with Scheme and Host
                        if (!image.Url.StartsWith("http"))
                        {
                            //If url is in // form then remove including host name
                            if (image.Url.StartsWith("//"))
                            {
                                image.Url = image.Url.Replace(string.Concat("//", pageUrl.Host), string.Empty);
                            }

                            //Now add Scheme and Host
                            image.Url = string.Format("{0}://{1}{2}{3}", pageUrl.Scheme, pageUrl.Host, !image.Url.StartsWith("/") ? "/" : string.Empty, image.Url);
                        }

                        image.Alt = imageAltAttribute?.Value ?? string.Empty;
                        image.Width = imageWidthAttribute?.Value ?? string.Empty;
                        image.Height = imageHeightAttribute?.Value ?? string.Empty;

                        if (!string.IsNullOrEmpty(image.Url))
                            images.Add(image);
                    }
                }
            }
            
        }

        return images;
    }

    private List<string> ExtractPageWords(HtmlDocument htmlDoc)
    {
        List<string> words = new List<string>();
        string text = htmlDoc.DocumentNode.SelectSingleNode("//body").InnerText;

        //Convert the text string into an array of words  
        string[] textWords = text.Split(new char[] { '.', '?', '!', ' ', ';', ':', ',' }, StringSplitOptions.RemoveEmptyEntries);  
        
        //Purge New Line and Tab chars
        const string reduceMultiSpace= @"[ ]{2,}";
        for(int i = 0; i < textWords.Count(); i++)
        {
            var word = textWords[i];
            var line = Regex.Replace(word.Replace("–", string.Empty).Replace("\n"," ").Replace("\t"," "), reduceMultiSpace, " ");
            if(!string.IsNullOrWhiteSpace(line))
            {
                if(line.IndexOf(" ") > -1)
                {
                    string[] innerWords = line.Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    foreach(string innerWord in innerWords)
                        words.Add(innerWord);
                }
                else
                {
                    words.Add(line);
                }
            }
        }

        return words;
    }

    private List<WordOccurance> CalculateWordOccurences(List<string> words, int topCount)
    {
        List<WordOccurance> wordOccurences = new List<WordOccurance>();
        
        foreach(string word in words)
        {
            WordOccurance? occurance = wordOccurences.FirstOrDefault(w => w.Value is not null && w.Value.Equals(word, StringComparison.InvariantCultureIgnoreCase));

            if(occurance == null)
            {
                int count = GetTermOccurance(word, words);
                wordOccurences.Add(new WordOccurance() {
                    Value = word,
                    Count = count
                });
            }                    
        }
        
        //Order Descenting putting the top occurences on the top
        wordOccurences = wordOccurences.OrderByDescending(w => w.Count).ToList();

        //Extract top count items
        wordOccurences = wordOccurences.Take(topCount).ToList();

        return wordOccurences;
    }

    private int GetTermOccurance(string searchTerm, List<string> source)
    {
        // Create the query.  Use the InvariantCultureIgnoreCase comparision to match "data" and "Data"
        var matchQuery = from word in source  
                         where word.Equals(searchTerm, StringComparison.InvariantCultureIgnoreCase)  
                         select word;
        // Count the matches, which executes the query.  
        int wordCount = matchQuery.Count();

        return wordCount;
    }
}
