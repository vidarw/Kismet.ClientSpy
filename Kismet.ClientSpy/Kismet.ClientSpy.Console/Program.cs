using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Kismet.ClientSpy.Client;
using Kismet.ClientSpy.Client.Classes;

namespace Kismet.ClientSpy.Console
{
    class Program
    {
        static void Main(string[] args)
        {
            var kismet = new KismetClient("192.168.1.198", 2501);
            Thread clientWorker = new Thread(kismet.ThreadRun);

            clientWorker.Start();

            while (true)
            {
                Update(kismet.WirelessClients);
                Thread.Sleep(1000);
            }
        }


        static void Update(List<WirelessClient> clients)
        {
            var onlineClients = clients.Where(x => x.LastSeen > DateTime.UtcNow.AddMinutes(-5)).ToList();

            System.Console.Clear();
            System.Console.WriteLine("|---------------------|---------------------|------------|------------|-------|");
            System.Console.WriteLine("|         Mac         |         Bssid       |   Owner    |   Seen     |  dBm  |");
            System.Console.WriteLine("|---------------------|---------------------|------------|------------|-------|");
            foreach (var wc in onlineClients)
            {
                var owner = wc.Owner + "            ";
                System.Console.WriteLine("|  " + wc.Mac + "  |  " + wc.Bssid + "  |  " + owner.Substring(0, 8) + "  |  " + wc.LastSeen.ToLongTimeString() + "  |  " + wc.Signal + "  |");
            }
            System.Console.WriteLine("|---------------------|---------------------|------------|------------|-------|");
            System.Console.WriteLine(DateTime.UtcNow);
        }
    }
}
