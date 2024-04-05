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

        public static async Task CreateUser(Container container, String emailAdd, String nickName, String profilePic, String userId )
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
            } catch(Exception ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }

    }
}
