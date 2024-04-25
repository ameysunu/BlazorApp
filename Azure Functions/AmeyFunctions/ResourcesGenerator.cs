using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace AmeyFunctions
{
    public class ResourcesGenerator
    {
        [FunctionName("ResourcesGenerator")]
        public static async Task Run([TimerTrigger("0 0 * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Resources Generator trigger fired at: {DateTime.UtcNow}");
            var articles = await GeminiController.GenerateInformativeArticles(log);
            log.LogInformation(articles);

            articles = articles.TrimStart().TrimEnd();
            articles = articles.Replace("json", "");
            articles = articles.Replace("`", "");

            try
            {
                List<Articles> articlesObject = JsonConvert.DeserializeObject<List<Articles>>(articles);
                await CosmosController.CreateArticlesInCosmos(articlesObject, log);
            } catch (Exception ex)
            {
                log.LogError($"Error occurred while trying to deserialize articles {ex.Message}");
            }

        }
    }
}
