using NetTools;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace Pinger
{
    class Scan
    {
        public IPAddress Address { get; set; }
        public int Timeout { get; set; }
        public bool PortScan { get; set; }

        public Scan(string ip, int timeout)
        {
            Address = IPAddress.Parse(ip);
            Timeout = timeout;
            PortScan = false;
        }

        public static IEnumerable<Scan> FromRange(string range, int timeout = 30)
        {
            return FromRange(IPAddressRange.Parse(range), timeout);
        }

        public static IEnumerable<Scan> FromRange(IPAddressRange range, int timeout = 30)
        {
            return range.AsEnumerable().Select(ip => new Scan(ip.ToString(), timeout));
        }

        public Task<ScanResult> Run()
        {
            return Task.Factory.StartNew(async () =>
            {
                var ping = await Ping();

                var result = new ScanResult
                {
                    IpAddress = this.Address,
                    Ping = ping
                };

                if (ping.Status != IPStatus.Success) return result;

                result.Dns = await Resolve();
                if (this.PortScan) result.Ports = await Ports();

                return result;
            }).Result;
        }

        public async Task<PingReply> Ping()
        {
            using (var ping = new Ping())
            {
                return await ping.SendPingAsync(this.Address, this.Timeout);
            }
        }

        public async Task<string> Resolve()
        {
            try
            {
                var dns = await Dns.GetHostEntryAsync(this.Address);
                return dns?.HostName;
            }
            catch (SocketException) { }

            return null;
        }

        public async Task<List<KeyValuePair<int, string>>> Ports()
        {
            return await Task.Factory.StartNew(() =>
            {
                var port = 1;
                var buffer = new byte[128];

                var results = new List<KeyValuePair<int, string>>();

                while (port < 100)
                {
                    var endpoint = new IPEndPoint(this.Address, port);

                    using (var socket = new Socket(endpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp)
                    {
                        ReceiveTimeout = this.Timeout
                    })
                    {
                        try
                        {
                            socket.Connect(endpoint);

                            //socket.Receive(buffer, buffer.Length, 0);

                            if (socket.Connected)
                            {
                                if (buffer.Length == 0)
                                {
                                    buffer = new byte[1024];
                                    socket.Send(buffer, buffer.Length, SocketFlags.None);
                                    socket.Receive(buffer);

                                    if (buffer.Length == 0) buffer = Encoding.UTF8.GetBytes("UNKNOWN");
                                }

                                var info = Encoding.UTF8.GetString(buffer);

                                results.Add(new KeyValuePair<int, string>(port, info));
                            }
                        }
                        catch
                        {
                            continue;
                        }
                        finally
                        {
                            port++;
                        }
                    }
                }

                return results;
            });
        }
    }
}