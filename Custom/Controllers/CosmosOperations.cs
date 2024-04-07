using Auth0.ManagementApi.Models;
using Microsoft.Azure.Cosmos;
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
}
