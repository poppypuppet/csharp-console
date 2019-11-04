using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Azure.Devices.Client;
using Newtonsoft.Json;

namespace console
{
    class LogLine
    {
        [JsonProperty]
        public string UTC { get; set; }
        [JsonProperty]
        public string LocalTime { get; set; }
        [JsonProperty]
        public string LogLevel { get; set; }
        [JsonProperty]
        public string Pid { get; set; }
        [JsonProperty]
        public string Class { get; set; }
        [JsonProperty]
        public string Data { get; set; }


    }
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Hello World!");
            string cs = "HostName=RTVIoTHuB.azure-devices.net;DeviceId=iotedgevm-east;SharedAccessKey=kd8fV/TQ2HaAyq6ZTfZNO47bejaWqE+cypgugjuS28k=";

            var csb = IotHubConnectionStringBuilder.Create(cs);
            Console.WriteLine(csb.DeviceId);
            Console.WriteLine(csb.GatewayHostName);
            Console.WriteLine(csb.HostName);
            Console.WriteLine(csb.ModuleId);
            Console.WriteLine(csb.SharedAccessKey);
            Console.WriteLine(csb.SharedAccessKeyName);
            Console.WriteLine(csb.SharedAccessSignature);

            //string file = @"C:\Users\weiwzhan\Desktop\logs.txt";
            //string archon = @"archon.log";
            string node = @"node.log";
            //string replacement = Regex.Replace(s, @"\t|\n|\r", "");
            int linecounter = 0;
            int matchcounter = 0;
            string line;



            // dd/MMM/yyyy:HH:mm:ss zzz
            // 2019-10-30T23:31:19.915433969Z 2019-10-30 23:31:19:915 - [verbose] - 6484 ArchonAG-graph1.ex1_a  - SingleEdgeIoGatherer::handleIncomingMessage  [handleIncomingMessage()][/go/src/RaaS.PerceptionEngine.Core/libArchon/archonag/./inputGatherer/singleEdgeIoGatherer.h:25] 
            // 2019-10-30T23:31:19.915433969Z 2019-10-30 23:31:19:915 [verbose] 6484 <ArchonAG-graph1.ex1_a> SingleEdgeIoGatherer::handleIncomingMessage  [handleIncomingMessage()][/go/src/RaaS.PerceptionEngine.Core/libArchon/archonag/./inputGatherer/singleEdgeIoGatherer.h:25] 
            // ^(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d+Z) (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}:\d{3}) - [(\a+)] - (\d+) .+

            Regex regex2 = new Regex(@"^(?<UTC>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d+Z)(?<DATA>.+)");
            Regex regex = new Regex(@"^(?<UTC>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d+Z) (?<LocalTime>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}:\d{3}) - \[(?<LOGLEVEL>\w+)\] - (?<PID>\d+) (?<CLASS>.+) - (?<DATA>.+)");
            // Read the file and display it line by line.  
            var logobj = new List<Dictionary<string, string>>();
            char[] charsToTrim = { ' ', '\n', '\r' };
            using (System.IO.StreamReader sr = new System.IO.StreamReader(node))
            {
                while ((line = sr.ReadLine()) != null)
                {
                    var dic = new Dictionary<string, string>();
                    var maches = regex.Matches(line);
                    if (regex.IsMatch(line))
                    {
                        maches = regex.Matches(line);
                    }
                    else if (regex2.IsMatch(line))
                    {
                        maches = regex2.Matches(line);
                    }

                    foreach (Match match in maches)
                    {
                        Console.WriteLine(match.Groups.Count);
                        foreach (string name in regex.GetGroupNames())
                        {
                            if (name != "0")
                            {
                                //dic[name] = Regex.Replace(match.Groups[name].Value, @"\t|\n|\r", "");
                                dic[name] = match.Groups[name].Value.Trim(charsToTrim);
                            }
                            Console.WriteLine("{0}:{1}", name, match.Groups[name]);
                        }
                        matchcounter++;
                    }
                    logobj.Add(dic);
                    linecounter++;
                }
            }
            Console.WriteLine(JsonConvert.SerializeObject(logobj));
            // end
            System.Console.WriteLine("There were {0} lines, {1} lines matched.", linecounter, matchcounter);
            System.Console.ReadLine();
        }
    }
}

