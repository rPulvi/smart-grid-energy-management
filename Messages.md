# Messages in the Grid #

The Peers in the mesh send messages each others to communicate.
Each message is a `MessageContract` class that inherits from the `PeerMessage` class.

![http://img405.imageshack.us/img405/9205/classdiagram.jpg](http://img405.imageshack.us/img405/9205/classdiagram.jpg)

Each message is made up an **Header** and a **Body**.

## Header Format ##

The Header contains these fields:

```
[DataMember]
public DateTime TimeStamp { get; set; }

[DataMember]
public Guid MessageID { get; set; }

[DataMember]
public String Sender { get; set; }

[DataMember]
public String Receiver { get; set; }

```

## Body Members ##

The Body of each messages contains specific information about the message. For example the amount of energy needed, the IP address, etc.

An example is the class `StatusNotifyMessage` that represents the message sent when a Peer look up energy.

```
[MessageContract]
public class StatusNotifyMessage : PeerMessage
{
    [MessageBodyMember]
    public PeerStatus status { get; set; }

    [MessageBodyMember]
    public float energyReq { get; set; }

}
```