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
            String name;
            
            if (args.Length > 0)
                name = args[0];
            else
                name = "Debug";
            
            Building casa = new Building(name, EnergyType.Solar, 100f);           
            
            Console.WriteLine("Starting Peer...");

            while (true)
            {
                Console.WriteLine("Energy level: {0}", casa.getEnergyLevel());

                Thread.Sleep(6000);
                casa.setLevel(90);                

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
