# Different Types of Energy #

Each `Building` can be a `Producer` or a `Consumer`
A `Consumer` is a Peer in the mesh that needs energy, while a `Producer` is a Peer which produces Energy to sell at a certain price.

Each node `Producer` is able to produce Energy in different ways:

  * Solar
  * Eolic
  * Hydric
  * Thermal

This aspect of the model is the described in the `EnergyType enum`

```
    public enum EnergyType
    {
        None = 0,
        Solar = 1,
        Eolic = 2,
        Hydric = 3,
        Thermic = 4,
    }
```

# Terminology #

In our application, we talk about:

  * **Energy Type:** Only for `Producer`, it means the type of Energy Production.
  * **Energy Peak:** The Maximum Energy required by the Building to work properly.
  * **Energy Produced:** The Energy produced by the Peer itself. For `Producers` this value must be greater than `Energy Peak`.