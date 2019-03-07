using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Greet;
using Grpc.Core;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;

namespace Secure_gRpc
{
    [Authorize(Policy = "protectedScope")]
    public class GreeterService : Greeter.GreeterBase
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public GreeterService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public override Task<HelloReply> SayHello(HelloRequest request, ServerCallContext context)
        {
            // Manual authorization as temp workaround
            var httpContext = _httpContextAccessor.HttpContext;
            var authOk = httpContext.User.HasClaim(claim => claim.Type == "scope" && claim.Value == "grpc_protected_scope");
            if (!authOk)
            {
                return Task.FromException<HelloReply>(
                    new RpcException(
                        new Status(StatusCode.Unauthenticated, "Incorrect token")));
            }

            return Task.FromResult(new HelloReply
            {
                Message = "Hello " + request.Name
            });
        }
    }
}
