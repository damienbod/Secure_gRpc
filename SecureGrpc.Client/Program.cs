using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using System.Security.Cryptography.X509Certificates;
using System.Threading.Tasks;
using Greet;
using Grpc.Core;
using Grpc.Net.Client;

namespace SecureGrpc.Client
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

            var channel = GrpcChannel.ForAddress("https://localhost:50051", new GrpcChannelOptions
            {
                HttpClient = CreateHttpClient()
            });

            CallOptions callOptions = new CallOptions(metadata);

            var client = new Greeter.GreeterClient(channel);

            var reply = await client.SayHelloAsync(
                new HelloRequest { Name = "GreeterClient" }, callOptions);

            Console.WriteLine("Greeting: " + reply.Message);

            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();
        }

        private static HttpClient CreateHttpClient()
        {
            var handler = new HttpClientHandler();
            var cert = new X509Certificate2(Path.Combine("Certs/client2.pfx"), "1111");
            handler.ClientCertificates.Add(cert);

            // Create client
            return new HttpClient(handler);
        }
    }
}
