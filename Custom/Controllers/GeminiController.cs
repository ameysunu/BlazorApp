using System;
using System.Security.Policy;
using System.Text;
using System.Text.Json;

namespace BlazorApp.Custom.Controllers
{
    public class GeminiController
    {

        public static async Task<String> SendRequestToGemini(string payload)
        {
            var configuration = new ConfigurationBuilder().AddJsonFile("appsettings.json", optional: false, reloadOnChange: true).Build();
            String apiKey = configuration["GeminiAPIKey"];
            String Url = configuration["GeminiEndpoint"] + "?key=" + apiKey;

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
                        Console.WriteLine(text);
                        return text;
                    }

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
        public static async Task<String> GenerateJournalSummary(String journalData)
        {
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
            return await SendRequestToGemini(jsonPayload);
        }

        public static async Task<String> GenerateMoodSuggestions(List<Double> moodData)
        {
            string jsonPayload = $@"
        {{
            ""contents"": [
                {{
                    ""parts"": [
                        {{
                            ""text"": ""Generate a mood suggestion 10 - 15 words sentence for the mood value: {moodData.Average()}, where on the scale of 0 to 15 - 0 being happiest and 15 being most unhappiest.""
                        }}
                    ]
                }}
            ]
        }}";
            return await SendRequestToGemini(jsonPayload);
        }

        public static async Task<String> GenerateMoodRecommendation(String text)
        {
            string jsonPayload = $@"
        {{
            ""contents"": [
                {{
                    ""parts"": [
                        {{
                            ""text"": ""I've been feeling {text}. Give me 10 recommendations of what I should do to improve my health?""
                        }}
                    ]
                }}
            ]
        }}";
            return await SendRequestToGemini(jsonPayload);
        }

        public static async Task<String> SendMessage(String text)
        {
            string jsonPayload = $@"
        {{
            ""contents"": [
                {{
                    ""parts"": [
                        {{
                            ""text"": ""{text}""
                        }}
                    ]
                }}
            ]
        }}";
            return await SendRequestToGemini(jsonPayload);
        }
    }

    public class Response
    {
        public List<Candidate> candidates { get; set; }
    }

    public class Content
    {
        public List<Part> parts { get; set; }
        public string role { get; set; }
    }

    public class Candidate
    {
        public Content content { get; set; }
        public string finishReason { get; set; }
        public int index { get; set; }
        public List<SafetyRating> safetyRatings { get; set; }
    }

    public class Part
    {
        public string text { get; set; }
    }

    public class SafetyRating
    {
        public string category { get; set; }
        public string probability { get; set; }
    }
}
