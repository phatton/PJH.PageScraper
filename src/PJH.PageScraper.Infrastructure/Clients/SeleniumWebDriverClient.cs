using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using OpenQA.Selenium.Chrome;
using OpenQA.Selenium;
using PJH.PageScraper.Core.Interfaces;

namespace PJH.PageScraper.Infrastructure.Clients
{
    public class SeleniumWebDriverClient : IScrapingClient
    {
        private readonly ILogger logger;

        public SeleniumWebDriverClient(ILogger<BrowserScrapingClient> logger)
        {
            this.logger = logger;
        }

        public Task<string> FetchHtml(Uri uri)
        {
            string result = "Nothing Happend";
            try
            {
                var chromeOptions = new ChromeOptions();
                chromeOptions.AddArguments(
                    "--headless",
                    "--no-sandbox",
                    "--disable-gpu",
                    "--whitelisted-ips"
                );
                var service = ChromeDriverService.CreateDefaultService("/usr/bin/", "chromedriver");
                using IWebDriver driver = new ChromeDriver(service, chromeOptions);
                driver.Navigate().GoToUrl(uri);
                driver.Manage().Timeouts().ImplicitWait = TimeSpan.FromMilliseconds(500);                
                result = driver.PageSource;
            }
            catch (Exception ex)
            {
                result = ex.Message;
            }

            return Task.FromResult(result);
        }
    }

}
