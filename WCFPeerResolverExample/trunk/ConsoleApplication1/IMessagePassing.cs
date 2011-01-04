using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;

namespace ConsoleApplication1
{
    [ServiceContract(CallbackContract=typeof(IMessagePassing))]
    public interface IMessagePassing
    {
        [OperationContract(IsOneWay = true)]
        void sendMsg(string message);
    }
}
