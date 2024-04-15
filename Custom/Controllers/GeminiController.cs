using System;
using System.Security.Policy;
using System.Text;

namespace BlazorApp.Custom.Controllers
{
    public class GeminiController
    {
        public static async Task<String> GenerateJournalSummary(String journalData)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
            String apiKey = configuration["GeminiAPIKey"];
            String Url = configuration["GeminiEndpoint"] + "?key=" + apiKey;
            string jsonPayload = $@"
        {{
            ""contents"": [
                {{
                    ""parts"": [
                        {{
                            ""text"": ""Write a 50 word summary regarding {journalData}""
                        }}
                    ]
                }}
            ]
        }}";

            using(HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Url);
                request.Content = new StringContent(jsonPayload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine(responseBody);
                    return responseBody;
                }
                else
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"Failed to call the API. Status code: {response.StatusCode}");
                    return "Error: " + responseBody;
                }
            }
        } 
    }
}
