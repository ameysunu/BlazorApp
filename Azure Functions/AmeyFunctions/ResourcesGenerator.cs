using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace AmeyFunctions
{
    public class ResourcesGenerator
    {
        [FunctionName("ResourcesGenerator")]
        public void Run([TimerTrigger("0 * * * * *", RunOnStartup = true)]TimerInfo myTimer, ILogger log)
        {
            log.LogInformation($"Resources Generator trigger fired at: {DateTime.UtcNow}");
        }
    }
}
