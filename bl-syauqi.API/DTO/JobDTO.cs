using Microsoft.Azure.Management.Media;
using System;
using System.Collections.Generic;
using System.Text;

namespace bl_syauqi.API.DTO
{
    public class JobDTO
    {
        public string jobName { get; set; }
        public string inputName { get; set; }
        public string outputName { get; set; }
        public string videoId { get; set; }
    }
}
