using Duplex;
using Grpc.Core;

namespace SecureGrpc.Server
{
    public class SubscribersModel
    {
        public IServerStreamWriter<MyMessage> Subscriber { get; set; }

        public string Name { get; set; }
    }
}
