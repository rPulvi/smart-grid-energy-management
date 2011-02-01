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
            String name = args[0];
            Building casa = new Building(name, EnergyType.Solar, 100f);           
            
            Console.WriteLine("Starting Peer...");

            while (true)
            {
                Console.WriteLine("Energy level: {0}", casa.getEnergyLevel());
                
                //string tmp = Console.ReadLine();
                //if (tmp == "")
                //{
                //    casa.StopEnergyProduction();
                //    break;
                //}

                Thread.Sleep(5000);
                casa.setLevel(90);                
            }

            casa.StopService();            
        }
    }
}
