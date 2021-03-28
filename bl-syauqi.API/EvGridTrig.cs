// Default URL for triggering event grid function in the local environment.
// http://localhost:7071/runtime/webhooks/EventGrid?functionName={functionname}
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.EventGrid.Models;
using Microsoft.Azure.WebJobs.Extensions.EventGrid;
using Microsoft.Extensions.Logging;

namespace bl_syauqi
{
    public static class EvGridTrig
    {
        [FunctionName("EvGridTrig")]
        [return: EventHub("msg.notification", Connection = "eventHubKey")]
        public static string Run([EventGridTrigger]EventGridEvent eventGridEvent, ILogger log)
        {
            var msg = eventGridEvent.Data.ToString();
            log.LogInformation(msg);
            return msg;
        }
    }
}
