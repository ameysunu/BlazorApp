using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AmeyFunctions
{
    public static class RecommendationEngine
    {
        [FunctionName("RecommendationEngine")]
        public static async Task<IActionResult> Run(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = null)] HttpRequest req,
            ILogger log)
        {
            log.LogInformation("Recommendation Engine was called using POST Request");

            string body = req.Query["body"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            body = data?.body;

            if (string.IsNullOrEmpty(body))
            {
                return new OkObjectResult("Request body was null");
            }

            string responseMessage = $"Hi there, recommendation engine body was: {body}";

            return new OkObjectResult(responseMessage);
        }
    }
}
