using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using SmartGridManager.Core.Commons;

namespace SmartGridManager
{
    class Program
    {
        static void Main(string[] args)
        {    
            String name;
            float price;

            if (args.Length > 0)
            {
                name = args[0];
                price = 0.9f;
            }
            else
            {
                name = "Debug";
                price = 1.2f;
            }
            
            Building casa = new Building(name, EnergyType.Solar, 100f, price);           
            
            Console.WriteLine("Starting Peer...");

            while (true)
            {
                //Console.WriteLine("Energy level: {0}", casa.getEnergyLevel());

                if (name == "Casa1")
                {
                    Thread.Sleep(6000);
                    casa.setEnergyLevel(90);
                }
                //Thread.Sleep(6000);

                //string tmp = Console.ReadLine();
                //if (tmp == "")
                //{
                //    casa.StopEnergyProduction();
                //    break;
                //}

            }

            casa.StopService();            
        }
    }
}
