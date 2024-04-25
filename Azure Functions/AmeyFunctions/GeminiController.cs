using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace AmeyFunctions
{
    public class GeminiController
    {
        public static async Task<String> SendRequestToGemini(string payload, ILogger log)
        {
            log.LogInformation("Preparing to send request to Gemini");

            var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
            .AddEnvironmentVariables()
            .Build();
            String apiKey = configuration["GeminiAPIKey"];
            String Url = configuration["GeminiEndpoint"] + "?key=" + apiKey;

            log.LogInformation($"Gemini URL recieved: {Url}");

            using (HttpClient client = new HttpClient())
            {
                HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, Url);
                request.Content = new StringContent(payload, Encoding.UTF8, "application/json");

                HttpResponseMessage response = await client.SendAsync(request);
                if (response.IsSuccessStatusCode)
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    var responseObj = JsonSerializer.Deserialize<Response>(responseBody);

                    if (responseObj?.candidates != null && responseObj.candidates.Count > 0)
                    {
                        var text = responseObj.candidates[0].content.parts[0].text;
                        log.LogInformation(text);
                        return text;
                    }

                    return responseBody;
                }
                else
                {
                    string responseBody = await response.Content.ReadAsStringAsync();
                    log.LogInformation($"Failed to call the API. Status code: {response.StatusCode}");
                    return "Error: " + responseBody;
                }
            }
        }

        public static async Task<String> GenerateMoodSummary(String moodData, ILogger log)
        {
            log.LogInformation("Initiating Mood Summary Process");
            string jsonPayload = $@"
        {{
            ""contents"": [
                {{
                    ""parts"": [
                        {{
                            ""text"": ""{moodData}. Give me some tips and recommendations based on this data.""
                        }}
                    ]
                }}
            ]
        }}";
            log.LogInformation($"JSON Payload: {jsonPayload}");
            return await SendRequestToGemini(jsonPayload, log);
        }

        public static async Task<String> GenerateJournalSummary(String journalData, ILogger log)
        {
            log.LogInformation("Initiating Journal Summary Process");
            string jsonPayload = $@"
        {{
            ""contents"": [
                {{
                    ""parts"": [
                        {{
                            ""text"": ""{journalData}. Give me some tips and recommendations based on this data.""
                        }}
                    ]
                }}
            ]
        }}";
            log.LogInformation($"JSON Payload: {jsonPayload}");
            return await SendRequestToGemini(jsonPayload, log);
        }

        public static async Task<String> GenerateInformativeArticles( ILogger log)
        {
            log.LogInformation("Generating Informative Articles");

            string jsonPayload = $@"
        {{
            ""contents"": [
                {{
                    ""parts"": [
                        {{
                            ""text"": ""Generate 10 informative articles about the benefits of practicing mindfulness for mental well-being. Separate these articles in a JSON format, as {{ public string id, public string title, public string url, public string description, public string created_on }} ""
                        }}
                    ]
                }}
            ]
        }}";
            return await SendRequestToGemini(jsonPayload, log);

        }
    }
}
