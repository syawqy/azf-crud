using Newtonsoft.Json;
using Nexus.Base.CosmosDBRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace bl_syauqi.DAL.Models
{
    public class ResourceVideo : ModelBase
    {
        [JsonProperty("fileName")]
        public string FileName { get; set; }
        [JsonProperty("type")]
        public string Type { get; set; }
        [JsonProperty("subject")]
        public string Subject { get; set; }
        [JsonProperty("status")]
        public string Status { get; set; }
        [JsonProperty("duration")]
        public string Duration { get; set; }
        [JsonProperty("containerId")]
        public string ContainerId { get; set; }
        [JsonProperty("streamingUrl")]
        public string[] StreamingUrl { get; set; }
        [JsonProperty("inputContainer")]
        public string InputContainer { get; set; }
        [JsonProperty("outputContainer")]
        public string OutputContainer { get; set; }
    }
}
