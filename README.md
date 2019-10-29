# TEST Akka.NET

## References

- [Akka Bootcamp](https://github.com/petabridge/akka-bootcamp)
- [Akka.NET API Doc](http://api.getakka.net/docs/stable/html/5590F8C9.htm)
- [Actors Vs Objects](https://anthonylebrun.silvrback.com/actors-vs-objects)
- [Akka.NET Stumbling Blocks](https://petabridge.com/blog/top-7-akkadotnet-stumbling-blocks/)
- [What problems does the actor model solve?](https://doc.akka.io/docs/akka/2.5.3/scala/guide/actors-intro.html)
- [What is Akka.NET?](https://getakka.net/articles/intro/what-is-akka.html)

## Actor Model

### But Why?

The Actor Model is a paradigm born to solve issues that OOP was not originally designed for. Instead of thinking of objects, we think in terms of actors that interact with each other by intermediary of ummutable messages. 

#### OOP & Multithreading

We rarely work with single-threaded applications. Very often, we have to work with long running jobs, blocking network or IO operations... and if you want to have a little bit of efficiency to your system, you would need to work with multi-threading. OOP was not really optimised for concurrency and in order to protect your objects' state of race conditions, we rely on locks (on a more or less abstracted way).

```csharp
class Test1
{    
     private IList<int> _data = new List<int>();

     public bool updateData(){
          _data.Add(_data.Count()); // would require a lock to work properly or use a concurrent collection
     }
}

class Program
{
     void main()
     {
          var test = new Test1();
	  Task.Run(() => (test.updateData());
	  Task.Run(() => (test.updateData());
     }
}
```

In the actor model approach, each actor has its own dedicated thread and memory, built-in into the frameworks. You won't have to manage yourselves the threads, each actor lives isolated from each other and share nothing: 

| Traditional OOP | Actor Model |
| ------------- | ------------- |
| ![traditional_oop_large](images/traditional_oop_large.png)  | ![actor_model_large](images/actor_model_large.png) |

#### Encapsulation

One of the important principle of OOP is encapsulation, the internal object state is only accessible through getters and setters from the other classes. By not letting the actors calling directly each other, the actor model covers the encapsulation in a more isolated way.

#### Distribution

Since the actors are built is a more isolated way, they share nothing (except contract definition) so they can be split into different services and as a matter of fact, machines. Distribution will allow a better scability of each individual component. 

### Main concepts

App1 solution is a sample code for discovering the main concepts. 

The aim of the application is to read all the files of a directory provided by the user. Each files contain a list of currencies for which we call an API to retrieve current FX rates. We define 4 actor type of actors: 
* **Console Reader Actor** (cr): who reads from the Console 
* **Console Writer Actor** (cw): who writes to the console
* **Currency Checker Actor** (cc): who will call the FX API
* **File Reader Actors** (fr0): spawned by the Console Reader for each file to read the file one row at a time and tell the currency to process to the currency checker

#### Actors Actions

Actors can:
* communicate with asynchronous messaging instead of method calls
* manage their own state
* when receiving a message: create child actors, send messages to other actors, supervise child actors

#### Context & Configuration

All actors live in a context, top level actors lives in the ActorSystem and children actors live in the parent context. The context holds metadata about the actors, for instance Sender's information. There should be only one ActorSystem base context by application as this is a very costly object to create. 

In order to initialize an actor, you define a Props configuration class. This Props class contains environment configurations that can be reused for deployment purpose (route mapping..). [HOCON configuration](https://getakka.net/articles/concepts/configuration.html) can be used to specify these options. 

#### Messaging

Actors communicate with each other by exchanging messages, rather than calling directly methods. 
In order to send messages to another actor, you must use its IActorRef (reference or handle to an actor) and the ActorSystem takes care of delivery the messages for you. After receiving a message, the actor do whatever it is dedicated to: send a request, write on disk...

![img](images/App1-02.png)

The messages will be received into the mailbox of the receiver actor (behavior isolated by the frameworks), in case the actor is not "available", busy or restarting, the message will be kept in the mailbox and distributed as soon as possible.

Another way of sending messages is to use explicitly its receiver address or ActorPath. This method called ActorSelection is not advised (see ["When Should I Use ActorSelection."](https://petabridge.com/blog/when-should-I-use-actor-selection/)). An address looks like this: 

```text
akka.tcp://MyActorSystem@localhost:9001/user/mynode/mychild
```
Example of use:
```csharp
Context.ActorSelection("akka://MyActorSystem/user/validationActor").Tell(message);
```

As an alternative, you can pass an IActorRef inside a message to hide the actor-implementation chosen from the receiver (and promote loose coupling). 

One constraint is that messages must be immutables. If they can't be made immutables for some reasons, enforcing serializing the messages in the configuration, will make sure that a new copy of the message is provided every time. 

```csharp
public class ImmutableMessage{
	public ImmutableMessage(string name, ReadOnlyList<int> points){
		Name = name;
		Points = points;
	}

	public string Name {get; private set;}
	public ReadOnlyList<int> Points {get; private set;}
}
```

#### Hierarchy & Supervision

Actors live in a hierarchy / a tree. Actors created from the ActorSystem are called "Guardians". The base actor (/user) is the Root Guardian. Other nodes are created from other parents, from their own contexts.

![img](images/App1-01.png)

Supervision is the mechanism of failure recovery in the actor system. Its aim is to contain and isolate the errors; and self-heal; without impacting any other actor, part of the system. Actors can spawn children, that are then under the supervision of their parent. In case of unhandled exception thrown by a child, the parent actor will be in charge of restarting, stopping or resuming the child; it can also decide to cascade the error to its own parent. These actions are called "directives". Two strategies are available: either only the responsible child is "punished" or all children are. This failover behavior is defined in the parent actor as you see fit. The framework will not allow an actor to crash the overall system. 

As we design our actor system, we will be pushing the "dangerous" actions (blocking or IO/network related). 

Note that if a parent is stopped, all children will be recursively stopped as well. 

#### Actor lifecycle

![lifecycle](\images\lifecycle_methods.png)

Because of the nature of actors, Dependency Injection will not necessarily work that well with the actors and can't be relied on.  

## Actor Model Frameworks in .NET

There are few actor model frameworks in .NET:
* Akka.NET
* Proto.Actor comes as a successor of Akka.NET. The original creator decided to rebuild another framework, to extend and improve Akka.Net.
* Orleans

[Benchmark Akka.NET vs Proto.Actor](https://github.com/Blind-Striker/actor-model-benchmarks)

### Akka.Net 

#### Cancellable long-running actions inside OnReceive

Out of the box, Akka.Net does not support very well awaitable methods and long-running actions. In an Actor System, the overall system is managed asynchronously, but the behavior when a message is received is expected to be synchronous. The main problem is that while a message is being processed, no other messages can be received, specially the "system" messages (such as exit command when stopping the application). There is workaround using a cancellable task and [pipes](https://petabridge.com/blog/akkadotnet-async-actors-using-pipeto/) available. 

#### Integration with the REST of the world

While the Akka.Remote and Akka.Cluster allows to deploy distributed actor systems, we can't enforce that all our apps will work with Actors. They are likely to use REST instead. Below are two projects give an example of integration of actors inside an ASP.NET Core project:
* [Proposed Implementation](https://havret.io/akka-net-asp-net-core) & [Code Sample](https://github.com/Havret/akka-net-asp-net-core)
* [Another Example](https://medium.com/@FurryMogwai/building-a-basket-micro-service-using-asp-net-core-and-akka-net-ea2a32ca59d5)

#### Distributed Services

Akka.Cluster is a layer of abstraction on top of Akka.Remote to give the ability of discovering members in the same cluster and using routers to balance the messages between actors in different nodes.

There are two types of routers: 

* Group Router: The actors to send the messages to — called routees — are specified using their actor path. The routers share the routees created in the cluster. 

![Group Router](\images\1_aRVBb-_v2dBpTV8m97Pd3w.png)

* Pool Router — The routees are created and deployed by the router, so they are its children in the actor hierarchy. Routees are not shared between routers. This is ideal for a master-slave scenario, where each router is the master and its routees the slaves.

![Pool Router](\images\1_ofa_x3hkM_sMzH5Nzum_Gg.png)

