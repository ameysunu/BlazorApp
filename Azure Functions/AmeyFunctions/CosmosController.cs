using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Azure;
using Microsoft.Azure.Cosmos;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Container = Microsoft.Azure.Cosmos.Container;

namespace AmeyFunctions
{
    public class CosmosController
    {
        private readonly CosmosClient _client;

        public CosmosController(string connectionString)
        {
            _client = new CosmosClient(connectionString);
        }
        public async Task<Container> InitializeUserDatabaseAndContainerAsync(string databaseId, string containerId)
        {
            Database database = await _client.CreateDatabaseIfNotExistsAsync(databaseId);
            Container container = await database.CreateContainerIfNotExistsAsync(containerId, "/userid");
            return container;
        }

        public static async Task<Container> GetCosmosContainer(String containerId)
        {
            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
            string connectionString = configuration["CosmosConnectionString"];
            string databaseId = configuration["CosmosDB"];


            var cosmosOperations = new CosmosController(connectionString);
            Container container = await cosmosOperations.InitializeUserDatabaseAndContainerAsync(databaseId, containerId);

            return container;

        }


        public static async Task CheckIfRecommendationExistsOrCreateAsync( String userId, String moodRecommendation, String journalRecommendation, ILogger log)
        {
            var container = await GetCosmosContainer("recommendations");
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.user_id = @PropertyValue")
    .WithParameter("@PropertyValue", userId);

            var queryIterator = container.GetItemQueryIterator<Recommendation>(query);
            var queryResponse = await queryIterator.ReadNextAsync();

            if (queryResponse.Count > 0)
            {
                log.LogInformation("Previous User Recommendation exists, proceeding to update");
                var itemId = queryResponse.First().id.ToString();
                ItemResponse<Recommendation> response = await container.ReadItemAsync<Recommendation>(itemId, PartitionKey.None);
                dynamic existingItem = response.Resource;

                if (existingItem != null)
                {
                    existingItem.mood_recommendation = moodRecommendation;
                    existingItem.journal_recommendation = journalRecommendation;
                    existingItem.created_at = DateTime.UtcNow.ToString();

                    await container.ReplaceItemAsync(existingItem, itemId, PartitionKey.None);
                    log.LogInformation($"Item Recommendation for user {userId} updated in DB successfully");
                    return;
                }
            }
            else
            {
                log.LogInformation("No previous User Recommendation found, proceeding to create");
                Recommendation newItem = new Recommendation
                {
                    id = Guid.NewGuid(),
                    created_at = DateTime.UtcNow.ToString(),
                    user_id = userId,
                    mood_recommendation = moodRecommendation,
                    journal_recommendation = journalRecommendation

                };

                try
                {
                    await container.CreateItemAsync(newItem);
                    log.LogInformation($"Item Recommendation for user {userId} written to DB successfully");
                }
                catch (Exception ex)
                {
                    log.LogInformation($"Exception occurred: {ex}");
                }
            }
        }

        public static async Task CreateArticlesInCosmos(List<Articles> articles, ILogger log)
        {
            var container = await GetCosmosContainer("articles");
            
            foreach(var article in articles)
            {
                try
                {
                    article.id = Guid.NewGuid().ToString();
                    article.created_on = DateTime.UtcNow.ToString();
                    await container.CreateItemAsync(article);
                    log.LogInformation($"Article created: {article.title}");
                } catch(Exception ex)
                {
                    log.LogError($"Exception: {ex.Message}");
                }

            }
        }
        
        public static async Task CreateBlogInCosmos(BlogPosts blogs, ILogger log)
        {
            var container = await GetCosmosContainer("blogposts");
            try
            {
                blogs.id = Guid.NewGuid().ToString();
                blogs.created_at = DateTime.UtcNow.ToString();
                await container.CreateItemAsync(blogs);
            } catch (Exception ex)
            {
                log.LogError($"Exception: {ex.Message}");
            }
        }
    }
}
