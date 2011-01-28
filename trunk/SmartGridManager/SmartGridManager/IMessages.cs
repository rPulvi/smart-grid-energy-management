﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ServiceModel;
using SmartGridManager.Messaging;

namespace SmartGridManager
{
    [ServiceContract(CallbackContract = typeof(IMessages))]
    public interface IMessages
    {
        [OperationContract(IsOneWay = true)]
        //void sayHello(myMessage m);
        void sayHello(GridMessage m);
    }
}