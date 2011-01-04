using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;
using System.Threading;
using System.Net;
using System.Net.PeerToPeer;


namespace ConsoleApplication1
{
    class Program
    {
        static void Main(string[] args)
        {
            if (Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count() <= 1)
            {
                for (int i = 0; i < 2; i++)
                {
                    Process.Start("ConsoleApplication1.exe");
                }
            }

            new Program().Run();
        }
            public void Run()
            {
                Console.WriteLine("Starting Demo");

                Console.WriteLine("Please, enter your name");
                String pname = Console.ReadLine();

                Peer myPeer = new Peer(pname);
                PeerNameRegistration reg = new PeerNameRegistration();
                reg.PeerName = myPeer;
                reg.Port = 2020;
                reg.Cloud = Cloud.Available;


                var peerThread = new Thread(myPeer.Run) { IsBackground = true };

                peerThread.Start();

                Thread.Sleep(500);

                reg.Start();
                Console.WriteLine("Registration:");
                Console.WriteLine("   Peer name: {0}", reg.PeerName.ToString());
                Console.WriteLine("   DNS  name: {0}", reg.PeerName.PeerHostName);
                Console.WriteLine();                
                while (true)
                {
                    Console.Write("Enter Name to Resolve ");
                    string tmp = Console.ReadLine();

                    Resolve(tmp);

                    if (tmp == "") break;                                           

                    //myPeer.Channel.sendMsg(tmp);                    
                }

                reg.Stop();
                myPeer.Stop();
                peerThread.Join();
            }
            public void Resolve(string name)
            {
                PeerNameResolver resolver = new PeerNameResolver();
                Peer peerName = new Peer (name);

                Console.WriteLine("Resolving {0}...", peerName);
                Console.WriteLine();

                PeerNameRecordCollection results = resolver.Resolve(peerName);

                if (results.Count == 0)
                {
                    Console.WriteLine("No records found.");
                    return;
                }

                int count = 1;
                foreach (PeerNameRecord record in results)
                {
                    Console.WriteLine("Record #{0}\n", count);
                    Console.WriteLine("DNS Name: {0}", record.PeerName.PeerHostName);

                    Console.WriteLine("Endpoints:");
                    foreach (IPEndPoint endpoint in record.EndPointCollection)
                    {
                        Console.WriteLine("\t Endpoint:{0}", endpoint);
                    }

                    if (record.Comment != null)
                    {
                        Console.WriteLine("Comment:");
                        Console.WriteLine(record.Comment);
                    }

                    if (record.Data != null)
                    {
                        Console.WriteLine("Data:");

                        // assumes the data is an ASCII formatted string
                        Console.WriteLine(
                            System.Text.Encoding.ASCII.GetString(record.Data));
                    }

                    count++;
                }
            }
    }
}
