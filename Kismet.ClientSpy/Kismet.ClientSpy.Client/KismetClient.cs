using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Kismet.ClientSpy.Client.Classes;
using System.Threading;

namespace Kismet.ClientSpy.Client
{
    public class KismetClient
    {
        public List<WirelessClient> WirelessClients { get; set; }

        public string Ip { get; set; }
        public int Port { get; set; }

        private DateTime _lastTime;
        private DateTime _currentTime;

        private bool _connected;

        public KismetClient(string ip, int port)
        {
            this.Init();
            Ip = ip;
            Port = port;
        }

        public void ThreadRun()
        {
            var client = new TcpClient(Ip, Port);
            var readBuffer = new byte[client.ReceiveBufferSize];
            var writeBuffer = new byte[client.SendBufferSize];

            using (var stream = client.GetStream())
            {
                //initial read;
                var initCount = stream.Read(readBuffer, 0, client.ReceiveBufferSize);
                var initString = Encoding.UTF8.GetString(readBuffer, 0, initCount);
                var initInfo = initString.Split('\n');

                //initial write;
                var cmd = "\n!0 enable client mac,bssid,firsttime,lasttime,signal_dbm\n";
                var encoding = new UTF8Encoding();
                writeBuffer = encoding.GetBytes(cmd);
                stream.Write(writeBuffer, 0, writeBuffer.Length);

                int readCount;
                while ((readCount = stream.Read(readBuffer, 0, client.ReceiveBufferSize)) != 0)
                {
                    var input = Encoding.UTF8.GetString(readBuffer, 0, readCount);
                    var rows = input.Split('\n');

                    foreach (var row in rows)
                    {
                        if (row.Contains("*TIME:"))
                        {
                            _lastTime = _currentTime;
                            _currentTime = DateTimeFromUnix(row.Substring(6).Trim());
                        }

                        if (row.Contains("*CLIENT:"))
                        {
                            var wlanClient = row.Substring(9).Split(' ');

                            if (wlanClient.Length < 5)
                            {
                                continue;
                            }

                            var mac = wlanClient[0];
                            var bssid = wlanClient[1];
                            var firstTime = DateTimeFromUnix(wlanClient[2]);
                            var lastTime = DateTimeFromUnix(wlanClient[3]);

                            var signal = -1;
                            int.TryParse(wlanClient[4], out signal);

                            var owner = "unknown";


                            if (WirelessClients.Any(x => x.Mac == mac))
                            {
                                var updateMe = WirelessClients.First(x => x.Mac == mac);
                                updateMe.Bssid = bssid;
                                updateMe.LastSeen = lastTime;
                                updateMe.Signal = signal;
                            }
                            else
                            {
                                if (mac.Contains("A3:2A:5C"))
                                {
                                    owner = "Vidar";
                                }

                                WirelessClients.Add(new WirelessClient()
                                {
                                    Mac = mac,
                                    Bssid = bssid,
                                    FirstSeen = firstTime,
                                    LastSeen = lastTime,
                                    Signal = signal,
                                    Owner = owner
                                });
                            }
                        }

                        if (row.Contains("*ACK:"))
                        {
                            Console.WriteLine("ACK Recieved: " + row.Substring(4));
                        }
                    }
                }
            }
        }

        private DateTime DateTimeFromUnix(string unixTime)
        {
            Double timeSpan;

            var dateTime = new System.DateTime(1970, 1, 1, 0, 0, 0, 0);
            var parseOk = Double.TryParse(unixTime, out timeSpan);

            if (parseOk)
            {
                return dateTime.AddSeconds(timeSpan);
            }

            return dateTime;
        }

        private void Init()
        {
            WirelessClients = new List<WirelessClient>();
        }

    }
}
