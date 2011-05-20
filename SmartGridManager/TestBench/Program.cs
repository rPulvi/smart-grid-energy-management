using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SmartGridManager.Core.Commons;
using SmartGridManager;
using System.Diagnostics;

namespace TestBench
{
    class Program
    {
        static void Main(string[] args)
        {
            int i = Process.GetProcessesByName(Process.GetCurrentProcess().ProcessName).Count();

            if (i <= 1)
                Process.Start("TestBench.exe", "c");

            if (args.Length > 0)
            {
                Program p = new Program();
                p.StartPeer("casa2", 0.9f, 0);
            }
            else
            {
                Program p = new Program();
                p.StartPeer("casa1", 1.2f, 1);
            }            
        }

        //type
        //0 = become a consumer
        //1 = else
        private void StartPeer(string name, float price, int type)
        {
            Building casa = new Building(name, EnergyType.Solar, 100f, price);
            
            Console.WriteLine("Starting Peer {0} ...",name);

            while (true)
            {                
                if (type == 1)
                {
                    Thread.Sleep(6000);
                    casa.setEnergyLevel(90);
                }
            }

            //casa.StopService();
        }
    }
}
