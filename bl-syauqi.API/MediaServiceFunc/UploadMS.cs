using System;
using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.Http;
using Microsoft.Extensions.Logging;
using Microsoft.WindowsAzure.Storage;
using Microsoft.WindowsAzure.Storage.Blob;
using Microsoft.Azure.Management.Media;
using Microsoft.Azure.Management.Media.Models;
using Newtonsoft.Json;
using Microsoft.IdentityModel.Clients.ActiveDirectory;
using Microsoft.Rest;
using Microsoft.Rest.Azure.Authentication;
using System.Linq;
using Azure.Storage.Blobs;
using System.Collections.Generic;
using Microsoft.Azure.WebJobs.Extensions.DurableTask;
using bl_syauqi.API.DTO;

namespace bl_syauqi.API
{
    public static class UploadMS
    {
        private static readonly string _azConn = Environment.GetEnvironmentVariable("azConnection");
        private static IAzureMediaServicesClient _client;

        [FunctionName("UploadVideo")]
        public static async Task<IActionResult> UploadVideo(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "UploadVideo")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            try
            {
                var formdata = await req.ReadFormAsync();
                var file = req.Form.Files["file"];

                string uniqueness = Guid.NewGuid().ToString("N");
                var aadclientid = "4542d90b-4ad0-4751-b8e9-d1c299d0b3bd";
                var aadsecret = "NJOWET1~-A09480f_uPl5SIjdZ.Wv3A_Pu";
                var aadtenantid = "0fa1f3f9-1330-425b-876a-9ffe2e503091";
                var armendpoint = "https://management.azure.com";
                var subid = "bda026af-224f-407e-b2fb-adeee91f9644";
                var rgname = "rg-beelingua-tutorial";
                var accname = "amsbltutorial";
                var assetname = $"input-{uniqueness}";

                ClientCredential clientCredential = new ClientCredential(aadclientid, aadsecret);
                ServiceClientCredentials cred = await ApplicationTokenProvider.LoginSilentAsync(aadtenantid, clientCredential, ActiveDirectoryServiceSettings.Azure);

                IAzureMediaServicesClient client = new AzureMediaServicesClient(new Uri(armendpoint), cred)
                {
                    SubscriptionId = subid,
                };
                _client = client;

                Asset asset = await client.Assets.CreateOrUpdateAsync(rgname, accname, assetname, new Asset());

                var response = await client.Assets.ListContainerSasAsync(
                    rgname,
                    accname,
                    assetname,
                    permissions: AssetContainerPermission.ReadWrite,
                    expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());

                var sasUri = new Uri(response.AssetContainerSasUrls.First());

                BlobContainerClient container = new BlobContainerClient(sasUri);
                BlobClient blob = container.GetBlobClient(Path.GetFileName(file.FileName));

                Stream fs = file.OpenReadStream();

                await blob.UploadAsync(fs);

                Asset outputAsset = await client.Assets.GetAsync(rgname, accname, assetname);
                Asset newasset = new Asset();
                string outputAssetName = assetname;

                if (outputAsset != null)
                {
                    string uniq = $"-{Guid.NewGuid():N}";
                    outputAssetName += uniq;

                    log.LogWarning("Warning – found an existing Asset with name = " + assetname);
                    log.LogWarning("Creating an Asset with this name instead: " + outputAssetName);
                }

                var updatedAsset = await client.Assets.CreateOrUpdateAsync(rgname, accname, outputAssetName, newasset);

                var tranfname = "bltutorial-syauqi";
                Transform transform = await client.Transforms.GetAsync(rgname, accname, tranfname);
                if (transform == null)
                {
                    // You need to specify what you want it to produce as an output
                    TransformOutput[] output = new TransformOutput[]
                    {
                        new TransformOutput
                        {
                            // The preset for the Transform is set to one of Media Services built-in sample presets.
                            // You can  customize the encoding settings by changing this to use "StandardEncoderPreset" class.
                            Preset = new BuiltInStandardEncoderPreset()
                            {
                                // This sample uses the built-in encoding preset for Adaptive Bitrate Streaming.
                                PresetName = EncoderNamedPreset.AdaptiveStreaming
                            }
                        }
                    };
                    // Create the Transform with the output defined above
                    transform = await client.Transforms.CreateOrUpdateAsync(rgname, accname, tranfname, output);
                }
                JobInput jobInput = new JobInputAsset(assetName: assetname);

                JobOutput[] jobOutputs =
                {
                    new JobOutputAsset(outputAssetName),
                };
                var jobname = $"job-bl-tutorial-syauqi-{uniqueness}";
                Job job = await client.Jobs.CreateAsync(
                    rgname,
                    accname,
                    tranfname,
                    jobname,
                    new Job
                    {
                        Input = jobInput,
                        Outputs = jobOutputs,
                    });

                JobDTO jobDTO = new JobDTO()
                {
                    jobName = jobname,
                    assetName = outputAssetName
                };
                string jsonobj = JsonConvert.SerializeObject(jobDTO);
                string instanceId = await starter.StartNewAsync("CheckJobStatus", input:jsonobj);

                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

                return starter.CreateCheckStatusResponse(req, instanceId);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
        [FunctionName("CheckJobStatus")]
        public static async Task<List<string>> CheckJobStatus(
            [OrchestrationTrigger] IDurableOrchestrationContext context)
        {
            string json = context.GetInput<string>();
            var outputs = new List<string>();

            // Replace "hello" with the name of your Durable Activity Function.
            outputs.Add($"Job Started at - {DateTime.Now}");
            Job job = await context.CallActivityAsync<Job>("cekUpdateJob", json);
            outputs.Add($"Job finished at - {DateTime.Now}");
            StreamingLocator sl = await context.CallActivityAsync<StreamingLocator>("getStreamLocator", json);
            outputs.Add($"get streamingLoctaor at - {DateTime.Now}");
            List<string> urls = await context.CallActivityAsync<List<string>>("getStreamingUrls", json);
            string urlString = String.Join(",", urls.ToArray());
            outputs.Add($"streamingLoctaor url - {urlString}");

            return outputs;
        }
        [FunctionName("cekUpdateJob")]
        public static async Task<Job> cekUpdateJob([ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            string json = context.GetInput<string>();
            JobDTO jobDTO = JsonConvert.DeserializeObject<JobDTO>(json);
            const int SleepIntervalMs = 20 * 1000;
            string rgname = "rg-beelingua-tutorial";
            string accname = "amsbltutorial";
            string tranfname = "bltutorial-syauqi";
            string jobName = jobDTO.jobName;

            Job job;
            do
            {
                job = await _client.Jobs.GetAsync(rgname, accname, tranfname, jobName);

                log.LogInformation($"Job is '{job.State}'.");
                for (int i = 0; i < job.Outputs.Count; i++)
                {
                    JobOutput output = job.Outputs[i];
                    log.LogInformation($"\tJobOutput[{i}] is '{output.State}'.");
                    if (output.State == JobState.Processing)
                    {
                        log.LogInformation($"  Progress (%): '{output.Progress}'.");
                    }
                }

                if (job.State != JobState.Finished && job.State != JobState.Error && job.State != JobState.Canceled)
                {
                    await Task.Delay(SleepIntervalMs);
                }
            }
            while (job.State != JobState.Finished && job.State != JobState.Error && job.State != JobState.Canceled);

            return job;
        }
        [FunctionName("getStreamLocator")]
        public static async Task<StreamingLocator> getStreamLocator([ActivityTrigger]
            IDurableActivityContext context,
            ILogger log)
        {
            string json = context.GetInput<string>();
            JobDTO jobDTO = JsonConvert.DeserializeObject<JobDTO>(json);
            string rgname = "rg-beelingua-tutorial";
            string accname = "amsbltutorial";
            string assetname = jobDTO.assetName;
            string locatorname = assetname.Replace("input","locator");
            StreamingLocator locator = new StreamingLocator();
            try
            {
                locator = await _client.StreamingLocators.CreateAsync(
                rgname,
                accname,
                locatorname,
                new StreamingLocator
                {
                    AssetName = assetname,
                    StreamingPolicyName = PredefinedStreamingPolicy.ClearStreamingOnly
                });
            }
            catch (Exception ex) 
            {
                log.LogError(ex.Message);
            }
            return locator;
        }

        [FunctionName("getStreamingUrls")]
        public static async Task<List<string>> getStreamingUrls([ActivityTrigger]
            IDurableActivityContext context,
            ILogger log)
        {
            string json = context.GetInput<string>();
            JobDTO jobDTO = JsonConvert.DeserializeObject<JobDTO>(json);
            string rgname = "rg-beelingua-tutorial";
            string accname = "amsbltutorial";
            string locatorname = jobDTO.assetName.Replace("input", "locator");
            const string DefaultStreamingEndpointName = "default";

            List<string> streamingUrls = new List<string>();

            StreamingEndpoint streamingEndpoint = await _client.StreamingEndpoints.GetAsync(rgname, accname, DefaultStreamingEndpointName);

            if (streamingEndpoint != null)
            {
                if (streamingEndpoint.ResourceState != StreamingEndpointResourceState.Running)
                {
                    await _client.StreamingEndpoints.StartAsync(rgname, accname, DefaultStreamingEndpointName);
                }
            }

            ListPathsResponse paths = await _client.StreamingLocators.ListPathsAsync(rgname, accname, locatorname);

            foreach (StreamingPath path in paths.StreamingPaths)
            {
                UriBuilder uriBuilder = new UriBuilder
                {
                    Scheme = "https",
                    Host = streamingEndpoint.HostName,

                    Path = path.Paths[0]
                };
                streamingUrls.Add(uriBuilder.ToString());
            }

            return streamingUrls;
        }
    }
}

