using System.Threading.Tasks;
using Greet;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;

namespace Secure_gRpc
{
    [Authorize(Policy = "protectedScope")]
    public class GreeterService : Greeter.GreeterBase
    {
        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
