using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading.Tasks;
using Greet;
using Grpc.Core;

namespace Secure_gRpc
{
    public class Program
    {
        static async Task Main(string[] args)
        {
            ///
            /// Token init
            /// 
            HttpClient httpClient = new HttpClient();
            ApiService apiService = new ApiService(httpClient);
            var token = await apiService.GetAccessTokenAsync();
            //var token = "This is invalid, I hope it fails";

            var tokenValue = "Bearer " + token;
            var metadata = new Metadata
            {
                { "Authorization", tokenValue }
            };

            ///
            /// Call gRPC method 1
            ///
            CallOptions callOptions = new CallOptions(metadata);
            // Include port of the gRPC server as an application argument
            var port = args.Length > 0 ? args[0] : "50051";
            var channel = new Channel("localhost:" + port, ChannelCredentials.Insecure);
            var client = new Greeter.GreeterClient(channel);

            var reply = await client.SayHelloAsync(
                new HelloRequest { Name = "GreeterClient" }, callOptions);

            Console.WriteLine("Greeting: " + reply.Message);

            await channel.ShutdownAsync();

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }
   }
}
