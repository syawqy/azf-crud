using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;

namespace bl_syauqi
{
    public static class DurFunc
    {
        [FunctionName("DurFunc")]
        public static async Task<List<string>> RunOrchestrator(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            var outputs = new List<string>();
            string tes = context.GetInput<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add(await context.CallActivityAsync<string>("DurFunc_Hello", "Pemesanan Kue"));
            outputs.Add(await context.CallActivityAsync<string>("DurFunc_Hello", "Kue dipersiapkan"));
            outputs.Add(await context.CallActivityAsync<string>("DurFunc_Hello", "Kue selesai dibuat"));
            outputs.Add(await context.CallActivityAsync<string>("DurFunc_Hello", "Kue dikirimkan"));
            outputs.Add(await context.CallActivityAsync<string>("DurFunc_Hello", "Kue diterima"));

            // returns ["Hello Tokyo!", "Hello Seattle!", "Hello London!"]
            return outputs;
        }

        [FunctionName("DurFunc_Hello")]
        public static string SayHello([ActivityTrigger] string name, ILogger log)
        {
            var today = DateTime.Now;
            var s = $"{name} - {today.ToLongTimeString()}";
            log.LogInformation(s);
            return s;
        }

        [FunctionName("DurFunc_HttpStart")]
        public static async Task<HttpResponseMessage> HttpStart(
            [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "DurableFunc")] HttpRequestMessage req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            // Function input comes from the request content.
            string instanceId = await starter.StartNewAsync("DurFunc", input:"tesasdf");

            log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

            return starter.CreateCheckStatusResponse(req, instanceId);
        }
    }
}