using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SmartGridManager
{
    public interface ITestChannel : ITest, IClientChannel
    {
    }

    class TestImplementation : ITest
    {
        public void testFunction(string s)
        {
            Console.WriteLine("Ho scritto {0} ", s);
        }
    }
}
