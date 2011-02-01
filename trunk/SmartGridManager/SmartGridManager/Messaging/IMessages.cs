using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SmartGridManager.Messaging;

namespace SmartGridManager.Messaging
{
    [ServiceContract(CallbackContract = typeof(IMessages))]
    public interface IMessages
    {
        [OperationContract(IsOneWay = true)]
        void sayHello(GridMessage m);

        [OperationContract(IsOneWay = true)]
        void statusAdv(AdvertisingMessage m);
    }
}
