#region Copyright notice and license

// Copyright 2019 The gRPC Authors
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

#endregion

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Duplex;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Secure_gRpc
{
    // no security, don't know how this works yet
    //[Authorize(Policy = "protectedScope")]
    public class DuplexService : Messaging.MessagingBase, IDisposable
    {
        private readonly ILogger _logger;

        public DuplexService(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.CreateLogger<DuplexService>();
        }

        private static HashSet<IServerStreamWriter<MyMessage>> _subscribers = new HashSet<IServerStreamWriter<MyMessage>>();

        public override async Task SendData(IAsyncStreamReader<MyMessage> requestStream, IServerStreamWriter<MyMessage> responseStream, ServerCallContext context)
        {
            var httpContext = context.GetHttpContext();
            _logger.LogInformation($"Connection id: {httpContext.Connection.Id}");

            if (!await requestStream.MoveNext())
            {
                // No messages so don't register and just exit.
                return;
            }

            var user = requestStream.Current.Name;

            // Warning, the following is very racy but good enough for a proof of concept
            // Register subscriber
            _logger.LogInformation($"{user} connected");
            _subscribers.Add(responseStream);

            do
            {
                await BroadcastMessageAsync(requestStream.Current, _logger);
            } while (await requestStream.MoveNext());

            _subscribers.Remove(responseStream);
            _logger.LogInformation($"{user} disconnected");
        }

        private static async Task BroadcastMessageAsync(MyMessage message, ILogger logger)
        {
            foreach (var subscriber in _subscribers)
            {
                logger.LogInformation($"Broadcasting: {message.Name} - {message.Message}");
                await subscriber.WriteAsync(message);
            }
        }

        public void Dispose()
        {
            _logger.LogInformation("Cleaning up");
        }
    }
}
