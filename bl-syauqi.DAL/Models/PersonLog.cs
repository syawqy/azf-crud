using Newtonsoft.Json;
using Nexus.Base.CosmosDBRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace bl_syauqi.DAL.Models
{
    public class PersonLog : ModelBase
    {
        [JsonProperty("name")]
        public string name { get; set; }
        [JsonProperty("data")]
        public string data { get; set; }
        [JsonProperty("city")]
        public string City { get; set; }
    }
}
