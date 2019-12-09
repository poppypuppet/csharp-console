using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.Azure.Devices.Client;
using Microsoft.Rest.Azure.Authentication;
using Newtonsoft.Json;

namespace console
{
    class Program
    {
        // dd/MMM/yyyy:HH:mm:ss zzz
        // 2019-10-30T23:31:19.915433969Z 2019-10-30 23:31:19:915 - [verbose] - 6484 ArchonAG-graph1.ex1_a  - SingleEdgeIoGatherer::handleIncomingMessage  [handleIncomingMessage()][/go/src/RaaS.PerceptionEngine.Core/libArchon/archonag/./inputGatherer/singleEdgeIoGatherer.h:25] 
        // 2019-10-30T23:31:19.915433969Z 2019-10-30 23:31:19:915 [verbose] 6484 <ArchonAG-graph1.ex1_a> SingleEdgeIoGatherer::handleIncomingMessage  [handleIncomingMessage()][/go/src/RaaS.PerceptionEngine.Core/libArchon/archonag/./inputGatherer/singleEdgeIoGatherer.h:25] 
        // ^(\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d+Z) (\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}:\d{3}) - [(\a+)] - (\d+) .+
        public static Regex regex2 = new Regex(@"(?<UTC>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d+Z)(?<DATA>.+)");
        public static Regex regex = new Regex(@"(?<UTC>\d{4}-\d{2}-\d{2}T\d{2}:\d{2}:\d{2}.\d+Z) (?<LocalTime>\d{4}-\d{2}-\d{2} \d{2}:\d{2}:\d{2}:\d{3}) - \[(?<LOGLEVEL>\w+)\] - (?<PID>\d+) (?<CLASS>.+) - (?<DATA>.+)");
        public static char[] charsToTrim = { ' ', '\n', '\r' };

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

            // 12/9/2019 9:16:50 PM
            Console.WriteLine(DateTime.Now.AddHours(-1).ToUniversalTime().ToString());
            // 12/9/2019 2:16:50 PM -08:00
            Console.WriteLine(DateTimeOffset.Now);
            // 2009-06-11T16:11:10.5312500Z
            DateTime.Now.ToUniversalTime().ToString("o");
            // 2009-06-11T17:11:10.5312500+0100
            DateTime.Now.ToString("o");

            DateTimeOffset dtf = DateTimeOffset.FromUnixTimeSeconds(1572917090);
            Console.WriteLine(dtf.UtcDateTime);
            Console.WriteLine(DateTime.Parse("2019-10-30T23:31:19.915433969Z"));

            foreach (PropertyDescriptor descriptor in TypeDescriptor.GetProperties(dtf))
            {
                string name = descriptor.Name;
                object value = descriptor.GetValue(dtf);
                Console.WriteLine("PropertyDescriptor {0}={1}", name, value);
            }

            Enume();
        }

        public static void Enume()
        {
            List<int> payloads = new List<int>();
            for (int i = 0; i < 100; i++)
            {
                payloads.Add(i);
            }

            IEnumerable<IEnumerable<int>> batches = payloads.Batch(10);
            foreach (var batch in batches)
            {
                foreach (var e in batch)
                {
                    Console.Write(e);
                }
                Console.WriteLine();
            }          
        }

        public static void logs()
        {
            var meta = new Dictionary<String, String>();
            meta.Add("ContainerId", "container.ID");
            meta.Add("ContainerImage", "container.Image");
            meta.Add("ContainerState", "container.State");
            var logsObj = new List<Dictionary<string, string>>();
            LogsMeta logsMeta = null;
            (logsMeta, logsObj) = PharseStream(meta);
            Console.WriteLine(JsonConvert.SerializeObject(logsMeta));
            Console.WriteLine(JsonConvert.SerializeObject(logsObj));
            Console.WriteLine(logsObj.Count);

            ThrottlePost(logsObj);

            // long[] timefilter = { 1572478279916, 1572478279917 };
            // Dictionary<LogFlag, string> valueFilters = new Dictionary<LogFlag, string>();
            // valueFilters.Add(LogFlag.CLASS, "NODE_EX1");
            // var res = FilterStream(logsObj, timefilter, valueFilters);
            // //Console.WriteLine(JsonConvert.SerializeObject(res));
            // Console.WriteLine(res.Count);

            string payloads = "{\"TS\":-1,\"TE\":-1,\"ID\":null,\"Interval\":1000,\"Filters\":{}}";
            var requestPayloadAsJson = JsonConvert.DeserializeObject<DirectMethodPayloads>(payloads);
            Console.WriteLine("Receive request payloads = {0}", JsonConvert.SerializeObject(requestPayloadAsJson));

            IList<LogFlag> flags = new List<LogFlag> { LogFlag.UTC, LogFlag.PID, LogFlag.LOGLEVEL, LogFlag.LOCALTIME, LogFlag.DATA, LogFlag.CLASS };

            Dictionary<LogFlag, string> valueFilters = new Dictionary<LogFlag, string>();
            Dictionary<LogFlag, string> ans = flags.Where(f => requestPayloadAsJson.Filters.ContainsKey(f.Value))
            .ToDictionary(f => f, f => requestPayloadAsJson.Filters[f.Value]);
            Console.WriteLine(JsonConvert.SerializeObject(ans));
        }

        public static void ThrottlePost(List<Dictionary<String, String>> payloads, int throttle = 10)
        {
            int length = payloads.Count;
            List<Object> temp = new List<Object>();
            for (int i = 0; i < length; i = i + throttle)
            {
                var items = payloads.Skip(i).Take(throttle);
                Console.WriteLine(JsonConvert.SerializeObject(items));
            }
        }

        public static (LogsMeta, List<Dictionary<String, String>>) PharseStream(Dictionary<String, String> meta)
        {
            //string archon = @"archon.log";
            string node = "color.log";
            //string replacement = Regex.Replace(s, @"\t|\n|\r", "");
            int lineCounter = 0;
            int matchCounter = 0;
            var logsObj = new List<Dictionary<string, string>>();

            using (System.IO.StreamReader sr = new System.IO.StreamReader(node))
            {
                string line;
                while ((line = sr.ReadLine()) != null)
                {
                    var dic = new Dictionary<String, String>();
                    var maches = regex.Matches(line);
                    if (!maches.Any())
                    {
                        maches = regex2.Matches(line);
                    }
                    // if (regex.IsMatch(line))
                    // {
                    //     // match full logs
                    //     maches = regex.Matches(line);

                    // }
                    // else if (regex2.IsMatch(line))
                    // {
                    //     // match logs only with timestamp
                    //     maches = regex2.Matches(line);
                    // }

                    foreach (Match match in maches)
                    {
                        foreach (string name in regex.GetGroupNames())
                        {
                            // "0" means whole string
                            if (name != "0")
                            {
                                //dic[name] = Regex.Replace(match.Groups[name].Value, @"\t|\n|\r", "");
                                dic[name] = match.Groups[name].Value.Trim(charsToTrim);
                            }
                        }
                        matchCounter++;
                    }
                    if (dic.Any())
                    {
                        foreach (var p in meta)
                        {
                            dic.Add(p.Key, p.Value);
                        }
                        logsObj.Add(dic);
                    }
                    lineCounter++;
                }
            }
            //Console.WriteLine(JsonConvert.SerializeObject(logsObj));
            return (new LogsMeta { ContainerId = meta["ContainerId"], LineCounter = lineCounter, MatchCounter = matchCounter }, logsObj);
        }

        public static List<Dictionary<String, String>> FilterStream(List<Dictionary<String, String>> data, long[] timefilter, Dictionary<LogFlag, string> valueFilters)
        {
            return data.Where(e =>
                 {
                     string ts = e[LogFlag.UTC.Value];
                     DateTime UnixStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, System.DateTimeKind.Utc);
                     long uts = (long)(DateTime.Parse(ts).ToUniversalTime() - UnixStart).TotalMilliseconds;
                     bool j = uts > timefilter[0] && uts <= timefilter[1];
                     return uts > timefilter[0] && uts <= timefilter[1];
                 })
                 .Where(e =>
                     valueFilters.All(p => e.ContainsKey(p.Key.Value) && p.Value == e[p.Key.Value])
                 ).ToList();
        }

        public class DirectMethodPayloads
        {
            public long TS { get; set; }

            public long TE { get; set; }

            public string ID { get; set; }

            public Dictionary<String, String> Filters { get; set; }
        }

        public class LogsMeta
        {
            public string ContainerId { get; set; }
            public long LineCounter { get; set; }
            public long MatchCounter { get; set; }
            public long FetchLogsDuration { get; set; }
            public long PhaseLogsDuration { get; set; }
            public long PostLogsDuration { get; set; }
        }

        public class LogFlag
        {
            private LogFlag(string value) { Value = value; }

            public string Value { get; set; }

            public static LogFlag UTC { get { return new LogFlag("UTC"); } }
            public static LogFlag LOCALTIME { get { return new LogFlag("LOCALTIME"); } }
            public static LogFlag LOGLEVEL { get { return new LogFlag("LOGLEVEL"); } }
            public static LogFlag PID { get { return new LogFlag("PID"); } }
            public static LogFlag CLASS { get { return new LogFlag("CLASS"); } }
            public static LogFlag DATA { get { return new LogFlag("DATA"); } }
        }
    }
}