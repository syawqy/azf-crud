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
using Microsoft.Azure.Cosmos;
using bl_syauqi.BLL;
using static bl_syauqi.DAL.Repository.Repositories;
using bl_syauqi.DAL.Models;
using System.Net;
using AzureFunctions.Extensions.Swashbuckle.Attribute;

namespace bl_syauqi.API
{
    public class UploadMS
    {
        private static readonly string _azConn = Environment.GetEnvironmentVariable("azConnection");
        private static IAzureMediaServicesClient _client;
        private static ClientCredential _clientCredential;
        private static string aadtenantid = "0fa1f3f9-1330-425b-876a-9ffe2e503091";
        private static string armendpoint = "https://management.azure.com";
        private static string subid = "bda026af-224f-407e-b2fb-adeee91f9644";
        private static string rgname = "rg-beelingua-tutorial";
        private static string accname = "amsbltutorial";
        private readonly CosmosClient _cosmosClient;

        public UploadMS(CosmosClient cosmosClient)
        {
            var aadclientid = "4542d90b-4ad0-4751-b8e9-d1c299d0b3bd";
            var aadsecret = "NJOWET1~-A09480f_uPl5SIjdZ.Wv3A_Pu";
            _clientCredential = new ClientCredential(aadclientid, aadsecret);
            _cosmosClient = cosmosClient;
        }

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
                
                var assetname = $"input-{uniqueness}";

                ServiceClientCredentials cred = await ApplicationTokenProvider.LoginSilentAsync(aadtenantid, _clientCredential, ActiveDirectoryServiceSettings.Azure);

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
                string outputAssetName = assetname.Replace("input","output");

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
                    outputName = outputAssetName
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
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(GetSaSDTO))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [FunctionName("GetSasUrl")]
        public async Task<IActionResult> GetSasUrl(
            [HttpTrigger(AuthorizationLevel.Function, "get", Route = "video/init")] HttpRequest req,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            try
            {
                string uniqueness = Guid.NewGuid().ToString("N");
                var videoService = new VideoService(new VideoRepository(_cosmosClient));

                var assetname = $"input-{uniqueness}";

                ServiceClientCredentials cred = await ApplicationTokenProvider.LoginSilentAsync(aadtenantid, _clientCredential, ActiveDirectoryServiceSettings.Azure);

                IAzureMediaServicesClient client = new AzureMediaServicesClient(new Uri(armendpoint), cred)
                {
                    SubscriptionId = subid,
                };
                _client = client;
                var newAsset = new Asset(name:assetname, container:assetname, description: "original file");
                Asset asset = await client.Assets.CreateOrUpdateAsync(rgname, accname, assetname, newAsset);

                var response = await client.Assets.ListContainerSasAsync(
                    rgname,
                    accname,
                    assetname,
                    permissions: AssetContainerPermission.ReadWrite,
                    expiryTime: DateTime.UtcNow.AddHours(4).ToUniversalTime());

                var sasUri = new Uri(response.AssetContainerSasUrls.First());

                var sasurlupload = sasUri.AbsoluteUri;

                ResourceVideo newvideo = new ResourceVideo()
                {
                    Type = "Video",
                    Subject = "test video",
                    Status = "Draft",
                    ContainerId = uniqueness,
                    InputContainer = asset.Container
                };
                var addedvideo = await videoService.CreatePerson(newvideo);

                var result = new GetSaSDTO()
                {
                    uploadUrl = sasurlupload,
                    videoId = addedvideo.Id
                };

                return new OkObjectResult(result);
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [FunctionName("RunEncode")]
        public async Task<IActionResult> RunEncode(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "video/encode")]
            [RequestBodyType(typeof(GetSaSDTO), "data")] GetSaSDTO data,
            [DurableClient] IDurableOrchestrationClient starter,
            ILogger log)
        {
            try
            {
                //var json = await req.ReadAsStringAsync();
                //var data = JsonConvert.DeserializeObject<GetSaSDTO>(json);
                var videoService = new VideoService(new VideoRepository(_cosmosClient));

                var datavideo = await videoService.GetVideoById(data.videoId);

                var jobDTO = new JobDTO()
                {
                    inputName = "input-"+datavideo.ContainerId,
                    videoId = datavideo.Id
                };
                var jobjson = JsonConvert.SerializeObject(jobDTO);

                string instanceId = await starter.StartNewAsync("CheckJobStatus",input:jobjson);

                log.LogInformation($"Started orchestration with ID = '{instanceId}'.");

                //return starter.CreateCheckStatusResponse(req, instanceId);
                return new OkObjectResult("Starting Encode");
            }
            catch (Exception ex)
            {
                return new BadRequestObjectResult(ex);
            }
        }
        [ProducesResponseType((int)HttpStatusCode.OK, Type = typeof(ResourceVideo))]
        [ProducesResponseType((int)HttpStatusCode.BadRequest, Type = typeof(string))]
        [ProducesResponseType((int)HttpStatusCode.NotFound, Type = typeof(string))]
        [FunctionName("GetStatus")]
        public async Task<IActionResult> GetStatus(
            [HttpTrigger(AuthorizationLevel.Function, "post", Route = "video/status")]
            [RequestBodyType(typeof(GetSaSDTO), "data")] GetSaSDTO data,
            ILogger log)
        {
            try
            {
                //var json = await req.ReadAsStringAsync();
                //var data = JsonConvert.DeserializeObject<GetSaSDTO>(json);
                var videoService = new VideoService(new VideoRepository(_cosmosClient));

                var datavideo = await videoService.GetVideoById(data.videoId);

                if(datavideo == null)
                {
                    return new NotFoundObjectResult("Data tidak ditemukan");
                }
                return new OkObjectResult(datavideo);
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

            outputs.Add($"Prepare for encoding at - {DateTime.Now}");
            JobDTO jobDTO = await context.CallActivityAsync<JobDTO>("StartEncode", json);
            json = JsonConvert.SerializeObject(jobDTO);
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
        [FunctionName("StartEncode")]
        public async Task<JobDTO> StartEncode([ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            string json = context.GetInput<string>();
            JobDTO jobDTO = JsonConvert.DeserializeObject<JobDTO>(json);
            var assetname = jobDTO.inputName;
            string tranfname = "bltutorial-syauqi-encode-2";

            var videoService = new VideoService(new VideoRepository(_cosmosClient));
            var datavideo = await videoService.GetVideoById(jobDTO.videoId);

            string outputAssetName = assetname.Replace("input", "output");
            Asset newasset = new Asset(name:outputAssetName,container:outputAssetName,description:"hasil encode");

            Asset cekAsset = await _client.Assets.GetAsync(rgname, accname, outputAssetName);

            if(cekAsset == null)
            {
                await _client.Assets.CreateOrUpdateAsync(rgname, accname, outputAssetName, newasset);
            }

            Transform transform = await _client.Transforms.GetAsync(rgname, accname, tranfname);
            if (transform == null)
            {
                TransformOutput[] output = new TransformOutput[]
                {
                        new TransformOutput
                        {
                            Preset = new StandardEncoderPreset(
                                    codecs: new Codec[]
                                    {
                                        new AacAudio(
                                            channels: 2,
                                            samplingRate: 48000,
                                            bitrate: 128000,
                                            profile: AacAudioProfile.AacLc
                                        ),

                                        new H264Video(
                                            layers:  new H264Layer[]
                                            {
                                                new H264Layer // Resolution: 1280x720
                                                {
                                                    Bitrate=1800000,
                                                    Width="1280",
                                                    Height="720",
                                                    Label="HD",
                                                },
                                                new H264Layer // YouTube 144p: 256×144
                                                {
                                                    Bitrate=64000,
                                                    Width="256",
                                                    Height="144",
                                                    Label="SD",
                                                }
                                            }),

                                        new JpgImage(
                                            start: "{Best}",
                            //                    step: "25%",
                            //                    range: "60%",
                                            layers: new JpgLayer[] {
                                                new JpgLayer(
                                                    width: "100%",
                                                    height: "100%"
                                                ),
                                            })
                                    },
                                    formats: new Format[]
                                    {
                                        new Mp4Format(
                                            filenamePattern:"Video-{Basename}-{Label}-{Bitrate}{Extension}"
                                        ),
                                        new JpgFormat(
                                            filenamePattern:"Thumbnail-{Basename}-{Index}{Extension}"
                                        )
                                    })
                        }
                };
                transform = await _client.Transforms.CreateOrUpdateAsync(rgname, accname, tranfname, output);
            }
            JobInput jobInput = new JobInputAsset(assetName: assetname);

            JobOutput[] jobOutputs =
            {
                new JobOutputAsset(outputAssetName),
            };
            var jobname = assetname.Replace("input", "job-encode-");
            Job job = await _client.Jobs.CreateAsync(
                rgname,
                accname,
                tranfname,
                jobname,
                new Job
                {
                    Input = jobInput,
                    Outputs = jobOutputs,
                });

            jobDTO.jobName = jobname;
            jobDTO.outputName = outputAssetName;

            datavideo.Status = "Encoding";
            datavideo.OutputContainer = newasset.Container;
            await videoService.UpdateVideo(datavideo);

            return jobDTO;
        }
        [FunctionName("cekUpdateJob")]
        public async Task<Job> cekUpdateJob([ActivityTrigger] IDurableActivityContext context,
            ILogger log)
        {
            string json = context.GetInput<string>();
            JobDTO jobDTO = JsonConvert.DeserializeObject<JobDTO>(json);
            const int SleepIntervalMs = 10 * 1000;
            string tranfname = "bltutorial-syauqi-encode-2";
            string jobName = jobDTO.jobName;

            var videoService = new VideoService(new VideoRepository(_cosmosClient));
            var datavideo = await videoService.GetVideoById(jobDTO.videoId);

            Job job;
            do
            {
                try
                {
                    job = await _client.Jobs.GetAsync(rgname, accname, tranfname, jobName);
                }
                catch
                {
                    throw new Exception("Job tidak ditemukan");
                }

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

            datavideo.Status = "Finished Encoding";
            await videoService.UpdateVideo(datavideo);

            return job;
        }
        [FunctionName("getStreamLocator")]
        public static async Task<StreamingLocator> getStreamLocator([ActivityTrigger]
            IDurableActivityContext context,
            ILogger log)
        {
            string json = context.GetInput<string>();
            JobDTO jobDTO = JsonConvert.DeserializeObject<JobDTO>(json);
            string assetname = jobDTO.outputName;
            string locatorname = assetname.Replace("output","locator");
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
        public async Task<List<string>> getStreamingUrls([ActivityTrigger]
            IDurableActivityContext context,
            ILogger log)
        {
            string json = context.GetInput<string>();
            JobDTO jobDTO = JsonConvert.DeserializeObject<JobDTO>(json);
            string locatorname = jobDTO.outputName.Replace("output", "locator");
            const string DefaultStreamingEndpointName = "default";

            var videoService = new VideoService(new VideoRepository(_cosmosClient));
            var datavideo = await videoService.GetVideoById(jobDTO.videoId);

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

            datavideo.StreamingUrl = streamingUrls.ToArray();
            await videoService .UpdateVideo(datavideo);

            return streamingUrls;
        }
    }
}

