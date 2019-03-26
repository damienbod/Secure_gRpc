using Duplex;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Threading.Tasks;

namespace SecureGrpc.Server
{
    public class ServerGrpcSubscribers
    {
        private readonly ILogger _logger;
        private ConcurrentDictionary<string, SubscribersModel> Subscribers = new ConcurrentDictionary<string,SubscribersModel>();

        public ServerGrpcSubscribers(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<ServerGrpcSubscribers>();
        }

        public async Task BroadcastMessageAsync(MyMessage message)
        {
            BlockingCollection<SubscribersModel> toRemove = new BlockingCollection<SubscribersModel>();
            foreach (var subscriber in Subscribers.Values)
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

        public void AddSubscriber(SubscribersModel subscriber)
        {
            bool added = Subscribers.TryAdd(subscriber.Name, subscriber);
            _logger.LogInformation($"New subscriber added: {subscriber.Name}");
            if(!added)
            {
                _logger.LogInformation($"could not add subscriber: {subscriber.Name}");
            }
        }

        public void RemoveSubscriber(SubscribersModel subscriber)
        {
            try
            {
                Subscribers.TryRemove(subscriber.Name, out SubscribersModel item);
                _logger.LogInformation($"Force Remove: {item.Name} - no longer works");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Could not remove {subscriber.Name}");
            }
        }
    }
}
