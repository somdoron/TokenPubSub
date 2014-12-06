using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;

namespace ExampleOne
{
    class Program
    {
        static void Main(string[] args)
        {
            using (var context = NetMQContext.Create())
            {
                string[] topics = new string[] { "A", "B", "C" };
                Random random = new Random();

                using (var publisher = context.CreateXPublisherSocket())
                {
                    // Set publisher to manual subscriptions mode
                    publisher.Options.ManualPublisher = true;
                    publisher.Bind("tcp://*:5556");
                    publisher.ReceiveReady += (sender, eventArgs) =>
                    {
                        var message = publisher.ReceiveString();

                        // we only handle subscription requests, unsubscription and any
                        // other type of messages will be dropped
                        if (message[0] == (char)1)
                        {
                            // calling Subscribe to add subscription to the last subscriber
                            publisher.Subscribe(message.Substring(1));
                        }
                    };

                    NetMQTimer sendTimer = new NetMQTimer(1000);
                    sendTimer.Elapsed += (sender, eventArgs) =>
                    {
                        // sends a message every second with random topic and current time
                        publisher.
                            SendMore(topics[random.Next(3)]).
                            Send(DateTime.Now.ToString());
                    };

                    Poller poller = new Poller();
                    poller.AddSocket(publisher);
                    poller.AddTimer(sendTimer);
                    poller.PollTillCancelled();
                }
            }
        }
    }
}
