using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;

namespace Pinger
{
    internal class ScanResult
    {
        public IPAddress IpAddress { get; set; }
        public PingReply Ping { get; set; }
        public string Dns { get; set; }
        public List<KeyValuePair<int, string>> Ports { get; set; }
    }
}