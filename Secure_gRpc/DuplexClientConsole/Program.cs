using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.IO;
using Duplex;

namespace DuplexClientConsole
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            if (args.Length != 1)
            {
                Console.WriteLine("No name provided. Usage: dotnet run <name>.");
                return 1;
            }

            var name = args[0];

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
            /// Call gRPC HTTPS
            ///
            var channelCredentials = new SslCredentials(
                File.ReadAllText("Certs\\ca.crt"),
                    new KeyCertificatePair(
                        File.ReadAllText("Certs\\client.crt"),
                        File.ReadAllText("Certs\\client.key")
                    )
                );

            CallOptions callOptions = new CallOptions(metadata);
            var port = "50051";

            var channel = new Channel("localhost:" + port, channelCredentials);
            var client = new Messaging.MessagingClient(channel);

            using (var chat = client.SendData())
            {
                Console.WriteLine($"Connected as {name}. Send empty message to quit.");

                // Dispatch, this could be racy
                var responseTask = Task.Run(async () =>
                {
                    while (await chat.ResponseStream.MoveNext(CancellationToken.None))
                    {
                        Console.WriteLine($"{chat.ResponseStream.Current.Name}: {chat.ResponseStream.Current.Message}");
                    }
                });

                var line = Console.ReadLine();
                while (!string.IsNullOrEmpty(line))
                {
                    await chat.RequestStream.WriteAsync(new MyMessage { Name = name, Message = line });
                    line = Console.ReadLine();
                }
                await chat.RequestStream.CompleteAsync();
            }

            Console.WriteLine("Shutting down");
            channel.ShutdownAsync().Wait();
            Console.WriteLine("Press any key to exit...");
            Console.ReadKey();

            return 0;
        }
    }
}
