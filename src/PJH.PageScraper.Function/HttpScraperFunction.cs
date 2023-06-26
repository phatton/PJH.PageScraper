using System.Net;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using PJH.PageScraper.Core.Interfaces;
using PJH.PageScraper.Core.Models;

namespace PJH.PageScraper.Function
{
    public class HttpScraperFunction
    {
        private readonly ILogger logger;
        private readonly IScrapingService scrapingService;

        public HttpScraperFunction(IScrapingService scrapingService, ILoggerFactory loggerFactory)
        {
            this.scrapingService = scrapingService;
            logger = loggerFactory.CreateLogger<HttpScraperFunction>();
        }

        [Function("HttpScraperFunction")]
        public async Task<HttpResponseData> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get", "post")] HttpRequestData req)
        {
            logger.LogInformation("C# HTTP trigger function processed a request.");
            
            PageData pageData = new PageData();

            //First attempt to retrieve Url from the QueryString on parameter q
            string? url = req.Query["q"];

            //Then Attempt to retrieve from the body json payload. Property q.
            if(string.IsNullOrEmpty(url))
            {
                try
                {
                    string requestBody = String.Empty;
                    using (StreamReader streamReader = new StreamReader(req.Body))
                    {
                        requestBody = await streamReader.ReadToEndAsync();
                    }

                    //Deserialize
                    dynamic data = JsonConvert.DeserializeObject(requestBody);
                    url = data?.q;
                }
                catch(Exception ex)
                {
                    this.logger.LogError(ex.Message);
                }
            }
            
            if(!string.IsNullOrEmpty(url)){
                //Retrieve PageData
                pageData = await this.scrapingService.GetPageDataAsync(url);
            }            

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Access-Control-Allow-Origin", "*");
            response.Headers.Add("Access-Control-Allow-Credentials", "true");
            await response.WriteAsJsonAsync(pageData); 

            return response;
        }
    }
}
