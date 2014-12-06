using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using NetMQ;
using Shared;

namespace AuthorizationService
{
  class Program
  {
    static void Main(string[] args)
    {
      // this key should be shared between authorization server and publisher
      const string Key = "SecretKey";

      using (var context = NetMQContext.Create())
      {
        using (var response = context.CreateResponseSocket())
        {
          response.Bind("tcp://*:5557");

          while (true)
          {
            var requestMessage = response.ReceiveMessage();

            string command = requestMessage.Pop().ConvertToString();

            if (command == AuthorizationProtocol.GetTokenCommand &&
                requestMessage.FrameCount == 3)
            {
              string username = requestMessage.Pop().ConvertToString();
              string password = requestMessage.Pop().ConvertToString();
              string subscription = requestMessage.Pop().ConvertToString();

              // TODO: validating username and password is not part 
              // of the example
              // TODO: validate that the user has permission to 
              // the subscription is not part of the example

              Console.WriteLine("Received GetTokenCommand {0} {1} {2}",
                  username, password, subscription);

              // Create a token
              Token token = new Token(subscription, Key);



              // send token to the client
              response.
                  SendMore(AuthorizationProtocol.SuccessReply).
                  Send(token.Serialize());
            }
            else
            {
              // unsupported command
              response.Send(AuthorizationProtocol.ErrorReply);
            }
          }
        }
      }
    }
  }
}
