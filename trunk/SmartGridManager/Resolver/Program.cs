using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.ServiceModel;


namespace Resolver
{
    class Program
    {
        static void Main(string[] args)
        {
            CustomResolver crs = new CustomResolver { ControlShape = false };
            ServiceHost customResolver = new ServiceHost(crs);
            
            crs.Open();
            customResolver.Open();

            Console.WriteLine("Custom resolver service is started");
            Console.WriteLine("Press <ENTER> to terminate service");
            Console.ReadLine();
        }
    }
}
