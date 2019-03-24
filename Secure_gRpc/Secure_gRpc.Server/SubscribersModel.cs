using Duplex;
using Grpc.Core;

namespace Secure_gRpc.Server
{
    public class SubscribersModel
    {
        public IServerStreamWriter<MyMessage> Subscriber { get; set; }

        public string Name { get; set; }
    }
}
