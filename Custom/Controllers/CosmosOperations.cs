﻿using Auth0.ManagementApi.Models;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Cosmos.Linq;
using System.Configuration;

namespace BlazorApp.Custom.Controllers
{
    public class CosmosOperations
    {

        private readonly CosmosClient _client;

        public CosmosOperations(string connectionString)
        {
            _client = new CosmosClient(connectionString);
        }

        public async Task<Container> InitializeUserDatabaseAndContainerAsync(string databaseId, string containerId)
        {
            Database database = await _client.CreateDatabaseIfNotExistsAsync(databaseId);
            Container container = await database.CreateContainerIfNotExistsAsync(containerId, "/userid");
            return container;
        }

        public static async Task<Container> ConnectToCosmosUserDB()
        {
            var configuration = new ConfigurationBuilder()
     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
     .Build();
            string connectionString = configuration["CosmosConnectionString"];
            string databaseId = configuration["CosmosDB"];
            string containerId = configuration["CosmosUserContainer"];


            var cosmosOperations = new CosmosOperations(connectionString);
            Container container = await cosmosOperations.InitializeUserDatabaseAndContainerAsync(databaseId, containerId);

            return container;

        }

        public static async Task CreateUser(Container container, String emailAdd, String nickName, String profilePic, String userId)
        {
            dynamic newItem = new
            {
                id = Guid.NewGuid().ToString(),
                created_at = DateTime.UtcNow,
                email = emailAdd,
                nickname = nickName,
                picture = profilePic,
                user_id = userId
            };

            try
            {
                await container.CreateItemAsync(newItem);
                Console.WriteLine("User written to DB successfully");
            } catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

        public static async Task<bool> CheckIfUserExists(Container container, String userId)
        {

            var query = new QueryDefinition($"SELECT VALUE COUNT(1) FROM c WHERE c.user_id = @PropertyValue")
                .WithParameter("@PropertyValue", userId);

            var queryIterator = container.GetItemQueryIterator<int>(query);
            var queryResponse = await queryIterator.ReadNextAsync();

            return queryResponse.Any() && queryResponse.First() > 0;
        }

        public static async Task<Container> GetCosmosContainer(String containerId)
        {
            var configuration = new ConfigurationBuilder()
     .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
     .Build();
            string connectionString = configuration["CosmosConnectionString"];
            string databaseId = configuration["CosmosDB"];


            var cosmosOperations = new CosmosOperations(connectionString);
            Container container = await cosmosOperations.InitializeUserDatabaseAndContainerAsync(databaseId, containerId);

            return container;

        }

        public static async Task<bool> CreateMood(Double mood_value, String moodReason, String userInfo)
        {
            var container = await GetCosmosContainer("moods");

            dynamic newItem = new
            {
                id = Guid.NewGuid().ToString(),
                created_at = DateTime.UtcNow,
                user_id = userInfo,
                mood = mood_value,
                mood_reason = moodReason,
            };

            try
            {
                await container.CreateItemAsync(newItem);
                Console.WriteLine("Mood written");
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine(ex.ToString());
                return false;
            }
        }

        public static async Task<List<Mood>> GetPresentMoods(String userId)
        {
            DateTime currentDate = DateTime.UtcNow;
            string formattedCurrentDate = currentDate.ToString("yyyy-MM-dd");
            List<Mood> presentMoodList = [];

            var container = await GetCosmosContainer("moods");

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.user_id = @PropertyValue")
                .WithParameter("@PropertyValue", userId);

            var queryIterator = container.GetItemQueryIterator<Mood>(query);
            var queryResponse = await queryIterator.ReadNextAsync();

            foreach (var mood in queryResponse)
            {
                DateTime createdAt = DateTime.Parse(mood.created_at);
                string createdAtString = createdAt.ToString("yyyy-MM-dd");

                if (createdAtString == formattedCurrentDate)
                {
                    presentMoodList.Add(mood);
                }
            }

            return presentMoodList;

        }

        public static async Task<List<Mood>> GetAllMoods(String userId)
        {
            List<Mood> allMoods = [];

            var container = await GetCosmosContainer("moods");
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.user_id = @PropertyValue")
    .WithParameter("@PropertyValue", userId);

            var queryIterator = container.GetItemQueryIterator<Mood>(query);
            var queryResponse = await queryIterator.ReadNextAsync();

            foreach (var mood in queryResponse)
            {
                allMoods.Add(mood);
            }

            return allMoods;
        }

        public static async Task<String> GetImagesFromMoodCosmos(double moodName)
        {
            Dictionary<double, string> moodEmojiMap = new Dictionary<double, string>
            {
                { 0, "happy" },
                { 0.5, "happy" },
                { 1, "happy" },
                { 1.5, "happy" },
                { 2, "happy" },
                { 2.5, "smiley" },
                { 3, "smiley" },
                { 3.5, "smiley" },
                { 4, "smiley" },
                { 4.5, "neutral" },
                { 5, "neutral" },
                { 5.5, "neutral" },
                { 6, "neutral" },
                { 6.5, "tearman" },
                { 7, "tearman" },
                { 7.5, "tearman" },
                { 8, "tearman" },
                { 8.5, "tired" },
                { 9, "tired" },
                { 9.5, "tired" },
                { 10, "tired" },
                { 10.5, "tensed" },
                { 11, "tensed" },
                { 11.5, "tensed" },
                { 12, "tensed" },
                { 12.5, "angry" },
                { 13, "angry" },
                { 13.5, "angry" },
                { 14, "angry" },
                { 14.5, "giveup" },
                { 15, "giveup" }
            };

            moodEmojiMap.TryGetValue(moodName, out string imageName);

            var container = await GetCosmosContainer("moodimages");

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.name = @PropertyValue")
                .WithParameter("@PropertyValue", imageName);

            var queryIterator = container.GetItemQueryIterator<MoodImage>(query);
            var queryResponse = await queryIterator.ReadNextAsync();

            foreach (var img in queryResponse)
            {
                return img.image;
            }

            return "";
        }

        public static async Task<String> DeleteMoodFromCosmos(Guid moodId)
        {
            var container = await GetCosmosContainer("moods");

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.id = @PropertyValue")
                .WithParameter("@PropertyValue", moodId.ToString());

            var queryIterator = container.GetItemQueryIterator<Mood>(query);
            var queryResponse = await queryIterator.ReadNextAsync();
            
            foreach(var response in queryResponse)
            {
                try
                {
                    await container.DeleteItemAsync<Mood>(response.id.ToString(), PartitionKey.None);
                    return "Succesfully deleted";
                    
                } catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return $"Error: {ex.Message}";
                }
            }

            return "";
        }

        public static async Task<String> CreateJournal(String title, String journalData, String userInfo)
        {
            var container = await GetCosmosContainer("journals");
            
            var summary = await GeminiController.GenerateJournalSummary(journalData);

            if (summary != null)
            {
                if (summary.Contains("Error"))
                {
                    return summary;
                }

                dynamic newItem = new
                {
                    id = Guid.NewGuid().ToString(),
                    created_at = DateTime.UtcNow,
                    title = title,
                    journalData = journalData,
                    user_id = userInfo,
                    summary = summary
                };

                try
                {
                    await container.CreateItemAsync(newItem);
                    Console.WriteLine("Journal created");
                    return "Success";
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                    return ex.ToString();
                }
            }

            return "Error";
        }

        public static async Task<List<Journal>> GetJournals(String userId)
        {
            List<Journal> allJournals = [];

            var container = await GetCosmosContainer("journals");
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.user_id = @PropertyValue")
    .WithParameter("@PropertyValue", userId);

            var queryIterator = container.GetItemQueryIterator<Journal>(query);
            var queryResponse = await queryIterator.ReadNextAsync();

            foreach (var journal in queryResponse)
            {
                allJournals.Add(journal);
            }

            return allJournals;
        }

        public static async Task<Journal> GetJournalById(String userId, string journalId)
        {
            Journal journalData = new Journal();

            var container = await GetCosmosContainer("journals");
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.user_id = @PropertyValue")
.WithParameter("@PropertyValue", userId);

            var queryIterator = container.GetItemQueryIterator<Journal>(query);
            var queryResponse = await queryIterator.ReadNextAsync();

            foreach (var journal in queryResponse)
            {
                if (journal.id.ToString() == journalId)
                {
                    journalData = journal;
                    return journalData;
                }
            }

            return journalData;
        }

        public static async Task<String> DeleteJournalById(Guid journalId)
        {
            var container = await GetCosmosContainer("journals");

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.id = @PropertyValue")
                .WithParameter("@PropertyValue", journalId.ToString());

            var queryIterator = container.GetItemQueryIterator<Journal>(query);
            var queryResponse = await queryIterator.ReadNextAsync();

            foreach (var response in queryResponse)
            {
                try
                {
                    await container.DeleteItemAsync<Journal>(response.id.ToString(), PartitionKey.None);
                    return "Successfully deleted";

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                    return $"Error: {ex.Message}";
                }
            }

            return "";
        }

        public static async Task<List<(String, String)>> GetAllMoodsForRecommendation(String userId)
        {
            DateTime currentDate = DateTime.UtcNow;
            List<(String, String)> moodList = new List<(String, String)>();

            var container = await GetCosmosContainer("moods");

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.user_id = @PropertyValue")
                .WithParameter("@PropertyValue", userId);

            var queryIterator = container.GetItemQueryIterator<Mood>(query);
            var queryResponse = await queryIterator.ReadNextAsync();

            foreach (var mood in queryResponse)
            {
                moodList.Add((mood.mood, mood.mood_reason));
            }

            return moodList;

        }

        public static async Task<List<String>> GetAllJournalDataForRecommendation(String userId)
        {
            List<String> journalData = [];

            var container = await GetCosmosContainer("journals");

            var query = new QueryDefinition($"SELECT * FROM c WHERE c.user_id = @PropertyValue")
                .WithParameter("@PropertyValue", userId);

            var queryIterator = container.GetItemQueryIterator<Journal>(query);
            var queryResponse = await queryIterator.ReadNextAsync();

            foreach (var journal in queryResponse)
            {
                journalData.Add((journal.journalData));
            }

            return journalData;

        }

        public static async Task<Recommendation> GetRecommendationByUserId(String userId)
        {

            var container = await GetCosmosContainer("recommendations");
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.user_id = @PropertyValue")
    .WithParameter("@PropertyValue", userId);

            var queryIterator = container.GetItemQueryIterator<Recommendation>(query);
            var queryResponse = await queryIterator.ReadNextAsync();

            if (queryResponse.Count > 0)
            {
                return queryResponse.First();
            }

            return null;
        }

        public static async Task<List<Articles>> GetAllArticles()
        {
            List<Articles> allArticles = [];
            var container = await GetCosmosContainer("articles");
            var query = new QueryDefinition($"SELECT * FROM c");

            var queryIterator = container.GetItemQueryIterator<Articles>(query);
            var queryResponse = await queryIterator.ReadNextAsync();

            foreach(var article in queryResponse)
            {
                allArticles.Add(article);
            }

            return allArticles;
        }

        public static async Task<List<BlogPosts>> GetAllBlogs()
        {
            List<BlogPosts> allBlogPosts = [];
            var container = await GetCosmosContainer("blogposts");
            var query = new QueryDefinition($"SELECT * FROM c");

            var queryIterator = container.GetItemQueryIterator<BlogPosts>(query);
            var queryResponse = await queryIterator.ReadNextAsync();

            foreach (var blog in queryResponse)
            {
                allBlogPosts.Add(blog);
            }

            return allBlogPosts;
        }

        public static async Task<BlogPosts> GetBlogById(string blogId)
        {
            BlogPosts blogPosts = new BlogPosts();

            var container = await GetCosmosContainer("blogposts");
            var query = new QueryDefinition($"SELECT * FROM c WHERE c.id = @PropertyValue")
.WithParameter("@PropertyValue", blogId);

            var queryIterator = container.GetItemQueryIterator<BlogPosts>(query);
            var queryResponse = await queryIterator.ReadNextAsync();

            foreach (var blog in queryResponse)
            {

                blogPosts = blog;
                return blogPosts;

            }

            return blogPosts;
        }

    }

    public class Articles
    {
        public string id { get; set; }
        public string title { get; set; }
        public string url { get; set; }
        public string description { get; set; }
        public string created_on { get; set; }

    }

    public class BlogPosts
    {
        public string id { get; set; }
        public string title { get; set; }
        public string content { get; set; }
        public string created_at { get; set; }
    }

    public class MoodImage
    {
        public Guid id { get; set; }
        public string name { get; set; }
        public string image { get; set; }
    }


    public class Mood
    {
        public Guid id { get; set; }
        public String created_at { get; set; }
        public string user_id { get; set; }
        public string mood_reason { get; set; }
        public string mood { get; set; }
    }

    public class Journal
    {
        public Guid id { get; set; }
        public String created_at { get; set; }
        public string user_id { get; set; }
        public string title { get; set;}
        public string journalData { get; set;}
        public string summary { get; set;}
    }

    public class Recommendation
    {
        public Guid id { get; set; }
        public string created_at { get; set; }
        public string user_id { get; set; }
        public string mood_recommendation { get; set; }
        public string journal_recommendation { get; set; }
    }
}
