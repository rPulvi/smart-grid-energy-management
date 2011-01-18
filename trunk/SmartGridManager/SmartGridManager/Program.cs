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
            Building casa = new Building(name, EnergyType.Solar, 4.2f);           
            
            Console.WriteLine("Starting Peer...");            

            while (true)
            {
                Console.WriteLine("Energy level: {0}", casa.getEnergyLevel());
                Console.WriteLine("Press any key to read another value");
                string tmp = Console.ReadLine();
                if (tmp == "")
                {
                    casa.StopEnergyProduction();
                    break;
                }
            }            

            while (true)
            {
                Console.WriteLine("Energy level: {0}", casa.getEnergyLevel());                
                string tmp = Console.ReadLine();
                if (tmp == "") break;
            }

            casa.StopService();            
        }
    }
}
