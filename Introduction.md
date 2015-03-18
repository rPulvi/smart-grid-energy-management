# Introduction #

This project aim to simulate a Smart Grid, offering an intelligent power distribution. The system is modelled above a P2P network.

Each node in the network is a `Building` (single home, private residences, commercial centre, etc.).
Every `Building` can be energy Producer or Consumer; we will use the term _Prosumers_.

Each `Building` is made up by a `PowerManager` and a `Generator`.

> `Building`
> > |
> > +----`PowerManager`
> > > |
> > > +-------`Generator`

The `PowerManager` is the main component which dynamically manages the energy distribution.
The `Generator` is a simple component that provides a numerical value indicating the energy produced. Every `Generator` may use several energy sources (solar, eolic, thermic, etc.) and this energy will be sold at different prices by each `Building`.

If the energy produced by a `Generator` can't satisfy the maximum energy required by the `Building`, the `PowerManager` will send a special request over the net and it will choose the best offer in terms of price and energy offered. Meanwhile, the others `Building` will receive the request and will evaluate a proposal if they have an energy surplus.

Once an agreement is established by the parts, they will monitor each other to check if the "connection" is still alive (heartbeat system) and restore the connection in case of fail.

# Scenario #
![http://img651.imageshack.us/img651/461/smartgrid.png](http://img651.imageshack.us/img651/461/smartgrid.png)