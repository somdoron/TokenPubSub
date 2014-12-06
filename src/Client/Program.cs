using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using Shared;

namespace Client
{
  class Program
  {
    static void Main(string[] args)
    {
      string username = args[0];
      string password = args[1];
      string subscription = args[2].ToUpper();

      using (var context = NetMQContext.Create())
      {
        string token;

        // first we try to get a token
        using (var request = context.CreateRequestSocket())
        {
          request.Connect("tcp://localhost:" + AuthorizationProtocol.Port);

          // send token request
          request.
              SendMore(AuthorizationProtocol.GetTokenCommand).
              SendMore(username).
              SendMore(password).
              Send(subscription);

          string result = request.ReceiveString();

          if (result == AuthorizationProtocol.SuccessReply)
          {
            token = request.ReceiveString();
          }
          else
          {
            throw new Exception("Invalid username or password");
          }
        }

        // we must use XSUB because
        using (var subscriber = context.CreateXSubscriberSocket())
        {
          subscriber.Connect("tcp://localhost:" + StreamingProtocol.Port);

          // create the subscription message
          byte[] subscriptionMessage = new byte[token.Length + 1];
          subscriptionMessage[0] = 1;
          Encoding.ASCII.GetBytes(token, 0, token.Length, subscriptionMessage, 1);
          subscriber.Send(subscriptionMessage);

          while (true)
          {
            string symbol = subscriber.ReceiveString();
            string price = subscriber.ReceiveString();

            Console.WriteLine("{0} {1}", symbol, price);
          }
        }
      }
    }
  }
}
