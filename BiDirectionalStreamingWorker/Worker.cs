using System;
using System.Threading;
using System.Threading.Tasks;
using Grpc.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System.Net.Http;
using System.IO;

namespace BiDirectionalStreamingWorker
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
                File.ReadAllText("Certs\\ca1.crt"),
                    new KeyCertificatePair(
                        File.ReadAllText("Certs\\client1.crt"),
                        File.ReadAllText("Certs\\client1.key")
                    )
                );

            var port = "50051";

            var name = "worker_client";
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation($"Worker running at: {DateTime.Now}");

                var channel = new Channel("localhost:" + port, channelCredentials);
                var client = new Duplex.Messaging.MessagingClient(channel);

                using (var sendData = client.SendData(metadata))
                {
                    Console.WriteLine($"Connected as {name}. Send empty message to quit.");

                    var responseTask = Task.Run(async () =>
                    {
                        while (await sendData.ResponseStream.MoveNext(stoppingToken))
                        {
                            Console.WriteLine($"{sendData.ResponseStream.Current.Name}: {sendData.ResponseStream.Current.Message}");
                        }
                    });

                    var line = Console.ReadLine();
                    while (!string.IsNullOrEmpty(line))
                    {
                        await sendData.RequestStream.WriteAsync(new Duplex.MyMessage { Name = name, Message = line });
                        line = Console.ReadLine();
                    }
                    await sendData.RequestStream.CompleteAsync();
                }

                await channel.ShutdownAsync();
                await Task.Delay(1000, stoppingToken);
            }
        }
    }
}
