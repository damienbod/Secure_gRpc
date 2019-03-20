using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.IO;

namespace DuplexClient
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;

        public Worker(ILogger<Worker> logger)
        {
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
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

            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Worker running at: {DateTime.Now}");

                //var channel = new Channel("localhost:50051", credentials);
                //var client = new Chatter.ChatterClient(channel);

                //using (var chat = client.Chat())
                //{
                //    Console.WriteLine($"Connected as {name}. Send empty message to quit.");

                //    // Dispatch, this could be racy
                //    var responseTask = Task.Run(async () =>
                //    {
                //        while (await chat.ResponseStream.MoveNext(CancellationToken.None))
                //        {
                //            Console.WriteLine($"{chat.ResponseStream.Current.Name}: {chat.ResponseStream.Current.Message}");
                //        }
                //    });

                //    var line = Console.ReadLine();
                //    while (!string.IsNullOrEmpty(line))
                //    {
                //        await chat.RequestStream.WriteAsync(new Duplex.MyMessage { Name = name, Message = line });
                //        line = Console.ReadLine();
                //    }
                //    await chat.RequestStream.CompleteAsync();
                //}

                //Console.WriteLine("Shutting down");
                //channel.ShutdownAsync().Wait();
                //Console.WriteLine("Press any key to exit...");
                //Console.ReadKey();


                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
