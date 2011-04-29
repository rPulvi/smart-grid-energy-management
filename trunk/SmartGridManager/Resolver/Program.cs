using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace Resolver
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomResolver crs = null;
            ServiceHost host = null;

            Resolver p = new Resolver("cambiarequestonome");

            Console.WriteLine("Press [ENTER] to exit.");
            Console.ReadLine();
        }

    }
}
