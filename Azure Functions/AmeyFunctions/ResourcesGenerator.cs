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

            var articlesJson = SanitizeStringForJson(articles);

            try
            {
                List<Articles> articlesObject = JsonConvert.DeserializeObject<List<Articles>>(articlesJson);
                await CosmosController.CreateArticlesInCosmos(articlesObject, log);
            } catch (Exception ex)
            {
                log.LogError($"Error occurred while trying to deserialize articles {ex.Message}");
            }

            var blogPosts = await GeminiController.GenerateBlogPosts(log);
            log.LogInformation(blogPosts);

            var blogsJson = SanitizeStringForJson(blogPosts);
            try
            {
                BlogPosts blogs = JsonConvert.DeserializeObject<BlogPosts>(blogsJson);
               await CosmosController.CreateBlogInCosmos(blogs, log);
            }
            catch (Exception ex)
            {
                log.LogError($"Error occurred while trying to deserialize blog {ex.Message}");
            }

        }

        private static string SanitizeStringForJson(string dirtyString)
        {
            dirtyString = dirtyString.TrimStart().TrimEnd();
            dirtyString = dirtyString.Replace("json", "");
            dirtyString = dirtyString.Replace("`", "");

            return dirtyString;
        }
    }
}
