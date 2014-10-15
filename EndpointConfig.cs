
using System;
using System.Threading;
using System.Threading.Tasks;

namespace SecondLevelRetryRace
{
    using NServiceBus;

	public class EndpointConfig : IConfigureThisEndpoint, AsA_Server, UsingTransport<SqlServer>
	{
    }

    class Starter : IWantToRunWhenBusStartsAndStops
    {
        public IBus Bus { get; set; }

        private Timer timer;

        public void Start()
        {
            Parallel.For(0, 5000, i =>
                Bus.SendLocal(new Foo()));

            timer = new Timer(Callback, null, 5000, 10000);
        }

        private void Callback(object state)
        {
            Console.Out.WriteLine("CurrentTotal={0}", FooHandler.number);
        }

        public void Stop()
        {
            using (var waitHandle = new ManualResetEvent(false))
            {
                timer.Dispose(waitHandle);
            }
        }
    }

    class FooHandler: IHandleMessages<Foo>
    {
        public static int number;

        public void Handle(Foo message)
        {
            var millisecond = DateTime.Now.Millisecond;
            if (millisecond >= 0 && millisecond < 500)
            {
                throw new Exception("No Foo");
            }

            Interlocked.Increment(ref number);
        }
    }

    class Foo: IMessage
    {
    }
}
