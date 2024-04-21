using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Collections.Generic;

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

            string moodData = req.Query["moodData"];
            string journalData = req.Query["journalData"];

            string requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            dynamic data = JsonConvert.DeserializeObject(requestBody);
            moodData = data?.moodData;
            journalData = data?.journalData;


            if (string.IsNullOrEmpty(moodData))
            {
                return new OkObjectResult("Mood Data body was null");
            }

            if(string.IsNullOrEmpty(journalData))
            {
                log.LogInformation("Journal data was empty");
            }

            var moodBuilder = MoodQueryBuilder(moodData);
            var journalBuilder = JournalQueryBuilder(journalData);

            log.LogInformation($"Mood Data was recieved as: {moodBuilder}, and Journal data is: {journalBuilder}");

            var geminiResponse = await GeminiController.GenerateMoodSummary(moodBuilder, log);

            return new OkObjectResult(geminiResponse);
        }
        public static string MoodQueryBuilder(string mood)
        {
            string baseResponder = "This is the list of my moods depicted on scale of 0 - 15, where 0 being the happiest and 15 being the saddest or angriest";
            baseResponder = baseResponder + mood;

            return baseResponder;
        }

        public static string JournalQueryBuilder(string journal)
        {
            string baseResponder = "This is my Journal data throughout the days";
            baseResponder = baseResponder + journal;

            return baseResponder;
        }
    }
}
