using Greet;
using Grpc.Core;
using Grpc.Net.Client;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http;
using System.Security.Cryptography.X509Certificates;
using System.Text.Json;
using System.Threading.Tasks;

namespace SecureGrpc.ManagedClient
{
    public class GrpcApiService
    {
        private readonly IOptions<AuthConfigurations> _authConfigurations;
        private readonly IHttpClientFactory _clientFactory;
        private readonly ApiTokenInMemoryClient _apiTokenInMemoryClient;

        public GrpcApiService(
            IOptions<AuthConfigurations> authConfigurations, 
            IHttpClientFactory clientFactory,
            ApiTokenInMemoryClient apiTokenClient)
        {
            _authConfigurations = authConfigurations;
            _clientFactory = clientFactory;
            _apiTokenInMemoryClient = apiTokenClient;
        }

        public async Task<string> GetGrpcApiDataAsync()
        {
            try
            {
                var client = _clientFactory.CreateClient("grpc");
                //var clientCertificate = new X509Certificate2("Certs/server.pfx", "1111");
                //var handler = new HttpClientHandler();
                //handler.ClientCertificates.Add(clientCertificate);
                //var client = new HttpClient(handler)
                //{
                //    BaseAddress = new Uri(_authConfigurations.Value.ProtectedApiUrl)
                //};

                var access_token = await _apiTokenInMemoryClient.GetApiToken(
                    "ProtectedGrpc",
                    "grpc_protected_scope",
                    "grpc_protected_secret"
                );

                var tokenValue = "Bearer " + access_token;
                var metadata = new Metadata
                {
                    { "Authorization", tokenValue }
                };

                CallOptions callOptions = new CallOptions(metadata);

                var channel = GrpcChannel.ForAddress(_authConfigurations.Value.ProtectedApiUrl);
                var clientGrpc = new Greeter.GreeterClient(channel);

                var response = await clientGrpc.SayHelloAsync(
                 new HelloRequest { Name = "GreeterClient managed" }, callOptions);

                return response.Message;
            }
            catch (Exception e)
            {
                throw new ApplicationException($"Exception {e}");
            }
        }
    }
}
