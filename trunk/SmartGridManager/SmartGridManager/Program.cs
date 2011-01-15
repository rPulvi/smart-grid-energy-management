using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace SmartGridManager
{
    class Program
    {
        static void Main(string[] args)
        {
            //EnergyGenerator eg = new EnergyGenerator(EnergyType.Hydric);

            //var peerthread = new Thread(eg.Start) { IsBackground = true };

            //peerthread.Start();
            //for (int i = 0; i < 19; i++)
            //{
            //    Thread.Sleep(3000);
            //    Console.WriteLine(eg.EnergyLevel);
            //}
            //eg.Stop();

            Console.WriteLine("Starting Peer...");
            Peer myPeer = new Peer("Rocco");
            myPeer.StartService();

            while (true)
            {
                Console.WriteLine("Press ENTER to kill Rocco");
                string tmp = Console.ReadLine();
                if (tmp == "") break;
            }

            myPeer.StopService();
        }
    }
}
