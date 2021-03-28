using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Azure.EventHubs;
using Microsoft.Azure.WebJobs;
using Microsoft.Extensions.Logging;
using bl_syauqi.DAL.Models;
using bl_syauqi.BLL;
using static bl_syauqi.DAL.Repository.Repositories;
using Microsoft.Azure.Documents.Client;

namespace bl_syauqi.API
{
    public static class EvtHubTrig
    {
        [FunctionName("EvtHubTrig1")]
        public static async Task EvtHubTrig1(
            [EventHubTrigger("person.notification", Connection = "eventHubKey")] EventData[] events,
            ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    PersonLogService personLogService = new PersonLogService(new PersonLogRepository());
                    var personLog = new PersonLog() {
                        name="tes",
                        data=messageBody,
                        City="Bandung"
                    };
                    await personLogService.CreatePersonLog(personLog);

                    // Replace these two lines with your processing logic.
                    log.LogInformation($"C# Event Hub trigger function processed a message: {messageBody}");
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }

        [FunctionName("EvtHubTrig2")]
        public static async Task EvtHubTrig2([EventHubTrigger("msg.notification", Connection = "eventHubKey")] EventData[] events, ILogger log)
        {
            var exceptions = new List<Exception>();

            foreach (EventData eventData in events)
            {
                try
                {
                    string messageBody = Encoding.UTF8.GetString(eventData.Body.Array, eventData.Body.Offset, eventData.Body.Count);
                    PersonLogService personLogService = new PersonLogService(new PersonLogRepository());
                    var personLog = new PersonLog() {
                        name="event 2",
                        data=messageBody,
                        City="Jakarta"
                    };
                    await personLogService.CreatePersonLog(personLog);

                    // Replace these two lines with your processing logic.
                    log.LogInformation($"receive message: {messageBody}");
                    await Task.Yield();
                }
                catch (Exception e)
                {
                    // We need to keep processing the rest of the batch - capture this exception and continue.
                    // Also, consider capturing details of the message that failed processing so it can be processed again later.
                    exceptions.Add(e);
                }
            }

            // Once processing of the batch is complete, if any messages in the batch failed processing throw an exception so that there is a record of the failure.

            if (exceptions.Count > 1)
                throw new AggregateException(exceptions);

            if (exceptions.Count == 1)
                throw exceptions.Single();
        }
    }
}
