using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using System.Net.Http;
using System.IO;
using Duplex;
using System.Security.Cryptography.X509Certificates;
using Grpc.Net.Client;

namespace BiDirectionalStreamingConsole
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var name = "BiDirectionalStreaming";
            if (args.Length != 1)
            {
                Console.WriteLine("No name provided. Using <BiDirectionalStreaming>");
                name = args[0];
            }

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

            var client = new Messaging.MessagingClient(channel);

            using (var duplex = client.SendData(metadata))
            {
                Console.WriteLine($"Connected as {name}. Send empty message to quit.");

                // Dispatch, this could be racy
                var responseTask = Task.Run(async () =>
                {
                    while (await duplex.ResponseStream.MoveNext(CancellationToken.None))
                    {
                        Console.WriteLine($"{duplex.ResponseStream.Current.Name}: {duplex.ResponseStream.Current.Message}");
                    }
                });

                var line = Console.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    await duplex.RequestStream.WriteAsync(new MyMessage { Name = name, Message = line });
                    line = Console.ReadLine();
                }
                await duplex.RequestStream.CompleteAsync();
            }

            Console.WriteLine("Shutting down");
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            return 0;
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
