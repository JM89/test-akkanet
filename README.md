# TEST Akka.NET

## References

- [Akka Bootcamp](https://github.com/petabridge/akka-bootcamp)
- [Akka.NET API Doc](http://api.getakka.net/docs/stable/html/5590F8C9.htm)
- [Actors Vs Objects](https://anthonylebrun.silvrback.com/actors-vs-objects)
- [Akka.NET Stumbling Blocks](https://petabridge.com/blog/top-7-akkadotnet-stumbling-blocks/)

## Actor Model

The actor model is a conceptual model to deal with concurrent computation. It defines some general rules for how the system's components should behave and interact with each other. 

Just like how everything is an object in OOP, in the actor model everything is an actor. Think of designing your system like a hierarchy of people, with tasks being split up and delegated until they become small enough to be handled concisely by one actor. 

How they differ: 
* [What problems does the actor model solve?
](https://doc.akka.io/docs/akka/2.5.3/scala/guide/actors-intro.html)

Other things to try:
* https://www.youtube.com/watch?v=6c1gVLyYcMM

### Proto.Actor

Proto.Actor is a Next generation Actor Model framework.

Proto.Actor was created by Roger Johansson, the original creator of Akka.NET. The reason for creating yet another actor model framework was due to the many design issues faced while building Akka.NET. Akka.NET required the team to build custom thread pools, custom network layers, custom serialization, custom configuration support and much more. All interesting topics on their own, but yield a huge cost in terms of development and maintenance hours.

Proto.Actor focuses on only solving the actual problems at hand, concurrency and distributed programming by reusing existing proven building blocks for all the secondary aspects.

Proto.Actor uses Protobuf for serialization, a decision that vastly simplifies the way Proto.Actor works. Message based systems should be about passing information, not passing complex OOP object graphs or code.

Proto.Actor also uses gRPC, leveraging HTTP/2 streams for network communication.

[Benchmark Akka.NET vs Proto.Actor](https://github.com/Blind-Striker/actor-model-benchmarks)

### Actors vs Objects

In a single-threaded program, (and implementation details aside) they are essentially identical. Having said that, most applications and systems that serve humans don't live in in this fantasy land of single-threaded execution. Long-running jobs, blocking IO and high volumes of concurrent connections that if handled naively in a single thread of execution would result in unusably slow applications, memory problems, crashes, and timeouts.

Traditional OOP languages weren't designed with concurrency as a first-class use case. While they do support the ability to spawn multiple threads, anyone who's done multi-threaded programming with these languages knows how easy it is to introduce race conditions (e.g. data desynchronization or corruption). 

```ruby
class DeepThought
  def initialize(state)
    @state = state
  end

  def meaning_of_life
    # This is a long running calculation where
    # @state gets mutated frequently to hold
    # intermediary calculations
  end
end

dt = DeepThought.new(data)

Thread.new { dt.meaning_of_life }
Thread.new { dt.meaning_of_life }

#=> BOOM!
```

Most conventional OOP languages provide locks or mutexes as the solution to this problem. It's as if the program is saying: "Hey guys, I'm doing stuff with this data, don't touch it until after I'm done".

Whereas you instantiate objects in the current thread of execution, you can think of creating actors as spawning a new thread/process for each actor created. Not only do actors persist indefinitely in their own thread, they also have their own dedicated memory space and share no memory with other actors.

If you remember nothing else, remember this: threads share state, actors share nothing.

![traditional_oop_large](images/traditional_oop_large.png)
![actor_model_large](images/actor_model_large.png)

Distribution: The second implication of share nothing is that technically actors don't have to live on the same machine. In fact, certain implementations of the actor model (like the Erlang VM) let you spawn actors transparently on different nodes. That is the beauty of the actor model: it redefines what concurrency is. Traditionally concurrency is thought of as using multiple cores on one machine at the same time. In the world of actors, the concept of concurrency not only includes scaling across CPU cores, but scaling across a computer network.

### Actors communication

Actors communicate with each other just as humans do, by exchanging messages. 

You code actors to handle messages they receive, and actors can do whatever you need them to in order to handle a message. Talk to a database, write to a file, change an internal variable, or anything else you might need.

In addition to processing a message it receives, an actor can:
* Create other actors
* Send messages to other actors (such as the Sender of the current message)
* Change its own behavior and process the next message it receives differently

All actors are created within a certain context. That is, they are "actor of" a context. The base context is defined by Akka.Actor.ActorSystem. The Context holds metadata about the current state of the actor, such as the Sender of the current message and things like current actors Parent or Children.

You never talk directly to an actor—you send messages to its IActorRef (reference or handle to an actor) and the ActorSystem takes care of delivering those messages for you. The purpose of an IActorRef is to support sending messages to an actor through the ActorSystem. 

#### Configuration

Props is a configuration class that encapsulates all the information needed to make an instance of a given type of actor. Props get extended to contain deployment information and other configuration details that are needed to do remote work. For example, Props are serializable, so they can be used to remotely create and deploy entire groups of actors on another machine somewhere on the network! Props support a lot of the advanced features (clustering, remote actors, etc) that give Akka.NET the serious horsepower which makes it interesting.

#### Supervision

Supervision is the basic concept that allows your actor system to quickly isolate and recover from failures. Every actor has another actor that supervises it, and helps it recover when errors occur. Every actor has a parent, and some actors have children (created from their own context). Parents supervise their children. The "guardians" are the root actors of the entire system, whenever you make an actor directly from the context of the actor system itself, that new actor is a top level actor. The / actor is the base actor of the entire actor system, and may also be referred to as "The Root Guardian." 

Every actor has an address. To send a message from one actor to another, you just have to know it's address or "ActorPath". 

For example, if we were running on localhost, the full address of actor b2 would be akka.tcp://MyActorSystem@localhost:9001/user/a2/b2 with a2 parent of b2. 

When things go wrong, that's when! Whenever a child actor has an unhandled exception and is crashing, it reaches out to its parent for help and to tell it what to do. Specifically, the child will send its parent a message that is of the Failure class. Then it's up to the parent to decide what to do.

When it receives an error from its child, a parent can take one of the following actions ("directives"). The supervision strategy maps different exception types to these directives, allowing you to handle different types of errors as appropriate.

Types of supervision directives (i.e. what decisions a supervisor can make): Restart the child (default); Stop the child (permanently terminates), Escalate the error (and stop itself): this is the parent saying "I don't know what to do! I'm gonna stop everything and ask MY parent!", Resume processing (ignores the error). There are two built-in supervision strategies: One-For-One says that the directive issued by the parent only applies to the failing child actor. All-For-One says that the directive issued by the parent applies to the failing child actor AND all of its siblings.

The whole point of supervision strategies and directives is to contain failure within the system and self-heal, so the whole system doesn't crash. We push potentially-dangerous operations from a parent to a child, whose only job is to carry out the dangerous task (such as a nasty network call).

Eg. Implementation of a file reader

![img](images/App1-01.png)

![img](images/App1-02.png)

#### Actor Selection

We know that we need a handle to an actor in order to send it a message and get it to do work. But now we have actors all over the place in this hierarchy, and don't always have a direct link (IActorRef) to the actor(s) we want to send messages to.

ActorSelection is nothing more than using an ActorPath to get a handle to an actor or actors so you can send them a message, without having to store their actual IActorRefs.

However, be aware that while ActorSelection is how you look up an IActorRef, it's not inherently a 1-1 lookup to a single actor.

In general, you should always try to use IActorRefs instead. But there are a couple of scenarios where ActorSelection are the right tool for the job and we cover those in more detail here: ["When Should I Use ActorSelection."](https://petabridge.com/blog/when-should-I-use-actor-selection/)

```csharp
Context.ActorSelection("akka://MyActorSystem/user/validationActor").Tell(message);
```

#### IActorRef being passed into message

Another alternative is to pass the IActorRef inside a message that is getting sent somewhere else in the system for processing. When that message is received, the receiving actor will know everything it needs to in order to do its job without the implementation details. This pattern actually promotes loose coupling.

### Actor lifecycle

![lifecycle](\images\lifecycle_methods.png)

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
