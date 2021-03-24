using Newtonsoft.Json;
using Nexus.Base.CosmosDBRepository;
using System;
using System.Collections.Generic;
using System.Text;

namespace bl_syauqi.Models
{
    public class Student : Person
    {
        [JsonProperty("studentId")]
        public string StudentId { get; set; }

        [JsonProperty("enterYear")]
        public string EnterYear { get; set; }
    }
}
