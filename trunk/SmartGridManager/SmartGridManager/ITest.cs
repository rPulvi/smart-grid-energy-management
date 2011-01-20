using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace SmartGridManager
{
    [ServiceContract(CallbackContract = typeof(ITest))]
    public interface ITest// : IClientChannel //, ICommunicationObject
    {
        [OperationContract(IsOneWay = true)]
        void testFunction(myMessage m);
    }
}
