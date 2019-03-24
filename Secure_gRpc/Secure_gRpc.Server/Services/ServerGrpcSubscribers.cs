using Duplex;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Secure_gRpc
{
   
    public class ServerGrpcSubscribers
    {
        private readonly ILogger _logger;
        public ServerGrpcSubscribers(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ServerGrpcSubscribers>();
        }

        public HashSet<IServerStreamWriter<MyMessage>> _subscribers = new HashSet<IServerStreamWriter<MyMessage>>();

        public async Task BroadcastMessageAsync(MyMessage message)
        {
            foreach (var subscriber in _subscribers)
            {
                _logger.LogInformation($"Broadcasting: {message.Name} - {message.Message}");
                await subscriber.WriteAsync(message);
            }
        }
    }
}
