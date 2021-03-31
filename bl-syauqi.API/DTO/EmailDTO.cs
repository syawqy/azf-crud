using System;
using System.Collections.Generic;
using System.Text;

namespace bl_syauqi.API.DTO
{
    public class EmailDTO
    {
        public string namauser { get; set; }
        public string subject { get; set; }
        public string email { get; set; }
        public List<string> listdata { get; set; }
    }
}
