using Duplex;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace SecureGrpc.Server
{
   
    public class ServerGrpcSubscribers
    {
        private readonly ILogger _logger;
        public HashSet<SubscribersModel> Subscribers = new HashSet<SubscribersModel>();

        public ServerGrpcSubscribers(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ServerGrpcSubscribers>();
        }

        public async Task BroadcastMessageAsync(MyMessage message)
        {
            List<SubscribersModel> toRemove = new List<SubscribersModel>();
            foreach (var subscriber in Subscribers)
            {
                var item = await SendMessageToSubscriber(subscriber, message);
                if (item != null)
                {
                    toRemove.Add(item);
                };
            }

            foreach (var subscriber in toRemove)
            {
                RemoveSubscriber(subscriber);
            }
        }

        private async Task<SubscribersModel> SendMessageToSubscriber(SubscribersModel subscriber, MyMessage message)
        {
            try
            {
                _logger.LogInformation($"Broadcasting: {message.Name} - {message.Message}");
                await subscriber.Subscriber.WriteAsync(message);
                return null;
            }
            catch(Exception ex)
            {
                _logger.LogError(ex, "Could not send");
                return subscriber;
            }
        }

        private void RemoveSubscriber(SubscribersModel subscriber)
        {
            try
            {
                Subscribers.Remove(subscriber);
                _logger.LogInformation($"Force Remove: {subscriber.Name} - no longer works");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not send");
            }
        }
    }
}
