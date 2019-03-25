using IdentityModel.Client;
using Microsoft.Extensions.Options;
using Newtonsoft.Json.Linq;
using System;
using System.Net.Http;
using System.Threading.Tasks;

namespace BiDirectionalStreamingWorker
{
    public class ApiService
    {
        private readonly HttpClient _tokenclient;

        private string stsServer = "https://localhost:44352";

        public ApiService(HttpClient httpClient)
        {
            _tokenclient = httpClient;
        }

        public async Task<string> GetAccessTokenAsync()
        {
            try
            {
                var disco = await HttpClientDiscoveryExtensions.GetDiscoveryDocumentAsync(_tokenclient, stsServer);

                if (disco.IsError)
                {
                    throw new ApplicationException($"Status code: {disco.IsError}, Error: {disco.Error}");
                }

                var tokenResponse = await HttpClientTokenRequestExtensions.RequestClientCredentialsTokenAsync(_tokenclient, new ClientCredentialsTokenRequest
                {
                    Scope = "grpc_protected_scope",
                    ClientSecret = "grpc_protected_secret",
                    Address = disco.TokenEndpoint,
                    ClientId = "ProtectedGrpc"
                });

                if (tokenResponse.IsError)
                {
                    throw new ApplicationException($"Status code: {tokenResponse.IsError}, Error: {tokenResponse.Error}");
                }

                return tokenResponse.AccessToken;

                //throw new ApplicationException($"Status code: {response.StatusCode}, Error: {response.ReasonPhrase}");
            }
            catch (Exception e)
            {
                throw new ApplicationException($"Exception {e}");
            }
        }
    }
}
