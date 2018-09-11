using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace ProQuant
{
    public class Job
    {
        public Job Deserialize(string json)
        {
            Job job = JsonConvert.DeserializeObject<Job>(json);
            return job;
        }

        public int job { get; set; }
        public int subjob { get; set; }
        public string status { get; set; }
        public DateTime created { get; set; }
        public string add1 { get; set; }
        public string add2 { get; set; }
        public string add3 { get; set; }
        public string add4 { get; set; }
        public string addpc { get; set; }
        public string awarded { get; set; }
        public string buildername { get; set; }
        public string description { get; set; }
        public int sentcount { get; set; }
        public double netValue { get; set; }
        public double grossValue { get; set; }
        public double vatValue { get; set; }


    }
}
