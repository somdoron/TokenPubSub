using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using NetMQ.Sockets;
using Shared;

namespace Publisher
{
  class Program
  {
    static void Main(string[] args)
    {
      // this key should be shared between authorization server and publisher
      const string Key = "SecretKey";

      string[] symbols = new[] {"EURUSD", "GBPUSD", "EURJPY", 
"USDJPY", "EURGBP", "GBPJPY"};

      Random random = new Random();

      using (var context = NetMQContext.Create())
      {
        using (var publisher = context.CreateXPublisherSocket())
        {
          publisher.Options.ManualPublisher = true;
          publisher.Bind("tcp://*:5558");
          publisher.ReceiveReady += (sender, eventArgs) =>
          {
            byte[] subscriptionBytes = publisher.Receive();

            // first byte indicate if it a subscription or unsubscription
            bool subscription = subscriptionBytes[0] == 1;

            if (subscription)
            {
              // the rest of the bytes is the token, convert them to string
              string serializedToken = Encoding.ASCII.GetString(
                  subscriptionBytes, 1, subscriptionBytes.Length - 1);

              // deserialize the token
              Token token;

              if (Token.TryDeserialize(serializedToken, out token))
              {
                // Check if the token is valid
                if (token.Validate(Key))
                {
                  if (subscription)
                  {
                    Console.WriteLine("Subscription request {0}",
                        token.Subscription);
                    publisher.Subscribe(token.Subscription);
                  }
                  else
                  {
                    publisher.Unsubscribe(token.Subscription);
                  }
                }
                else
                {
                  Console.WriteLine("Invalid token {0}",
                      serializedToken);
                }
              }
            }
          };

          // Some fake publishing
          NetMQTimer publishTimer = new NetMQTimer(100);
          publishTimer.Elapsed += (sender, eventArgs) =>
          {
            publisher.
                SendMore(symbols[random.Next(symbols.Length)]).
                Send(random.Next().ToString());
          };

          Poller poller = new Poller();
          poller.AddSocket(publisher);
          poller.AddTimer(publishTimer);
          poller.PollTillCancelled();
        }
      }
    }
  }
}
