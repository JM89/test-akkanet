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

#### Hierarchy & Supervision

Actors live in a hierarchy / a tree. Actors created from the ActorSystem are called "Guardians". The base actor (/user) is the Root Guardian. Other nodes are created from other parents, from their own contexts.

![img](images/App1-01.png)

Supervision is the mechanism of failure recovery in the actor system. Its aim is to contain and isolate the errors; and self-heal; without impacting any other actor, part of the system. Actors can spawn children, that are then under the supervision of their parent. In case of unhandled exception thrown by a child, the parent actor will be in charge of restarting, stopping or resuming the child; it can also decide to cascade the error to its own parent. These actions are called "directives". Two strategies are available: either only the responsible child is "punished" or all children are. This failover behavior is defined in the parent actor as you see fit. The framework will not allow an actor to crash the overall system. 

As we design our actor system, we will be pushing the "dangerous" actions (blocking or IO/network related). 

Note that if a parent is stopped, all children will be recursively stopped as well. 

#### Actor lifecycle

![lifecycle](\images\lifecycle_methods.png)

## Actor Model Frameworks in .NET

There are few actor model frameworks in .NET:
* Akka.NET
* Proto.Actor comes as a successor of Akka.NET. The original creator decided to rebuild another framework, to extend and improve Akka.Net.
* Orleans

[Benchmark Akka.NET vs Proto.Actor](https://github.com/Blind-Striker/actor-model-benchmarks)

## Integration with the REST of the world

### Distributed Services

### Integration to ASP.NET Core

# Notes

### Stumbling blocks

#### Messages are immutables

One of the fundamental principles of designing actor-based systems is to make 100% of all message classes immutable, meaning that once you allocate an instance of that object its state can never be modified again.

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

**Hack**:
If you can't make the class immutable, there is a [HOCON configuration](https://getakka.net/articles/concepts/configuration.html) setting  you can turn on that will force each message to be serialized and deserialized to each actor who receives it, which guarantees that each actor receives their own unique copy of the message (akka.actor.serialize-messages). Intended for testing only, serialization is costly.

#### Cancellable long-running actions inside OnReceive

Actors process exactly one message at a time inside their Receive method, as shown below. This makes it extremely simple to program actors, because you never have to worry about race conditions affecting the internal state of an actor when it can only process one message at a time.

Unfortunately, there’s a price you pay for this: if you stick a long-running operation inside your Receive method then your actors will be unable to process any messages, including system messages, until that operation finishes. And if it’s possible that the operation will never finish, it’s possible to deadlock your actor.

The solution to this is simple: you need to encapsulate any long-running I/O-bound or CPU-bound operations inside a Task and make it possible to cancel that task from within the actor.

```csharp
public class FooActor : ReceiveActor,
						IWithUnboundedStash{

	private Task _runningTask;
	private CancellationTokenSource _cancel;

	public IStash Stash {get; set;}

	public FooActor(){
		_cancel = new CancellationTokenSource();
		Ready();
	}

	private void Ready(){
		Receive<Start>(s => {
			var self = Self; // closure
			_runningTask = Task.Run(() => {
				// ... work
			}, _cancel.Token).ContinueWith(x =>
			{
				if(x.IsCancelled || x.IsFaulted)
					return new Failed();
				return new Finished();
			}, TaskContinuationOptions.ExecuteSynchronously)
			.PipeTo(self);

			// switch behavior
			Become(Working);
		})
	}

	private void Working(){
		Receive<Cancel>(cancel => {
			_cancel.Cancel(); // cancel work
			BecomeReady();
		});
		Receive<Failed>(f => BecomeReady());
		Receive<Finished>(f => BecomeReady());
		ReceiveAny(o => Stash.Stash());
	}

	private void BecomeReady(){
		_cancel = new CancellationTokenSource();
		Stash.UnstashAll();
		Become(Ready);
	}
}
```

Cf. [Using Pipe](https://petabridge.com/blog/akkadotnet-async-actors-using-pipeto/)

#### Can't rely on DI

Some actors live very short lives - they might only be used for a single request before they’re shutdown and discarded. Other actors of the same type can potentially live forever and will remain in memory until the application process is terminated. This is a problem for most DI containers as most of them expect to work with objects that have fairly consistent lifecycles - not a blend of both. On top of that, many disposable resources such as database connections get recycled in the background as a result of connection pooling - so your long-lived actors may suddenly stop working if you’re depending on a DI framework to manage the lifecycle of that dependency for you.

Thus it’s considered to be a good practice for actors to manage their own dependencies, rather than delegate that work to a DI framework.

If an actor needs a new database connection, it’s better for the actor to fetch that dependency itself than to trust that the DI framework will do the right thing. Because most DI frameworks are extremely sloppy about cleaning up resources and leak them all over the place, as we’ve verified through extensive testing and framework comparisons. The only DI framework that works correctly by default with Akka.NET actors is Autofac.

#### Avoid awaiting async methods

Actors are inherently asynchronous and concurrent - every time you send a message to an actor you’re dispatching work asynchronously to it. But because the async and await keywords aren’t there, a commonly held view among some new Akka.NET users is that therefore the IActorRef.Tell operation must be “blocking.” This is incorrect - Tell is asynchronous; it puts a message into the back of the destination actor’s mailbox and moves on. The actor will eventually process that message once it makes it to the front of the mailbox.

Moreover, inside many Receive methods we see end users develop lots of nested async / await operations inside an individual message handler. There’s a cost overlooked by most users to doing this: the actor can’t process any other messages between each await operation because those awaits are still part of the “1 message at a time” guarantee for the original message!

## Communications with other apps

### Add a REST layer

How to expose your actor based system to the world? You can try to do so by leveraging Akka.Remote package with its location transparency feature, but it would imply that all your clients use Akka.NET as well. It’s one hell of an assumption in this day and age when microservices rule the world. So we go for REST, then. 

[Proposed Implementation](https://havret.io/akka-net-asp-net-core)
[Code Sample](https://github.com/Havret/akka-net-asp-net-core)
[Another Example](https://medium.com/@FurryMogwai/building-a-basket-micro-service-using-asp-net-core-and-akka-net-ea2a32ca59d5)

### Akka Cluster

https://github.com/petabridge/akkadotnet-code-samples/tree/master/Cluster.WebCrawler
https://www.freecodecamp.org/news/how-to-make-a-simple-application-with-akka-cluster-506e20a725cf/

An akka cluster represents a fault-tolerant, elastic, decentralized peer-to-peer network of Akka.NET applications with no single point of failure or bottleneck

Akka.Cluster is a layer of abstraction on top of Akka.Remote, that puts Remoting to use for a specific structure: clusters of applications. Under the hood, Akka.Remote powers Akka.Cluster, so anything you could do with Akka.Remote is also supported by Akka.Cluster.

Akka Cluster gives you out-of-the-box the discovery of members in the same cluster. Using Cluster Aware Routers it is possible to balance the messages between actors in different nodes. It is also possible to choose the balancing policy, making load-balancing a piece of cake!

#### Types of routers:

* Group Router: The actors to send the messages to — called routees — are specified using their actor path. The routers share the routees created in the cluster. 

![Group Router](\images\1_aRVBb-_v2dBpTV8m97Pd3w.png)


* Pool Router — The routees are created and deployed by the router, so they are its children in the actor hierarchy. Routees are not shared between routers. This is ideal for a master-slave scenario, where each router is the master and its routees the slaves.

![Pool Router](\images\1_ofa_x3hkM_sMzH5Nzum_Gg.png)

#### Use Cases

Akka.Cluster lends itself naturally to high availability scenarios.

To put it bluntly, you should use clustering in any scenario where you have some or all of the following conditions:
* A sizable traffic load
* Non-trivial to perform
* An expectation of fast response times
* The need for elastic scaling (e.g. bursty workloads)
* A microservices architecture

Some of the use cases where Akka.Cluster emerges as a natural fit are in:
* Analytics
* Marketing Automation
* Multiplayer Games
* Devices Tracking / Internet of Things
* Alerting & Monitoring Systems
* Recommendation Engines
* Dynamic Pricing
* ...
