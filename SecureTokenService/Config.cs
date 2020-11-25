// Copyright (c) Brock Allen & Dominick Baier. All rights reserved.
// Licensed under the Apache License, Version 2.0. See LICENSE in the project root for license information.

using IdentityServer4;
using IdentityServer4.Models;
using Microsoft.Extensions.Configuration;
using System.Collections.Generic;

namespace StsServerIdentity
{
    public class Config
    {
        public static IEnumerable<ApiScope> GetApiScopes()
        {
            return new List<ApiScope>
            {
                new ApiScope("grpc_protected_scope", "grpc_protected_scope")
            };
        }

        public static IEnumerable<IdentityResource> GetIdentityResources()
        {
            return new List<IdentityResource>
            {
                new IdentityResources.OpenId(),
                new IdentityResources.Profile(),
                new IdentityResources.Email(),
                new IdentityResource("my_identity_scope",new []{ "role", "admin", "user" } )
            };
        }

        public static IEnumerable<ApiResource> GetApiResources()
        {
            return new List<ApiResource>
            {
                new ApiResource("ProtectedGrpc")
                {
                    DisplayName = "API protected",
                    ApiSecrets =
                    {
                        new Secret("grpc_protected_secret".Sha256())
                    },
                    Scopes = new List<string>{"grpc_protected_scope" },
                    UserClaims = { "role", "admin", "user", "safe_zone_api" }
                }
            };
        }

        public static IEnumerable<Client> GetClients(IConfigurationSection authConfigurations)
        {
            return new List<Client>
            {
                new Client
                {
                    ClientId = "ProtectedGrpc",
                    ClientName = "ProtectedGrpc",
                    ClientSecrets = new List<Secret> { new Secret { Value = "grpc_protected_secret".Sha256() } },
                    AllowedGrantTypes = GrantTypes.ClientCredentials,
                    AllowedScopes = new List<string> { "grpc_protected_scope" }
                }
            };
        }
    }
}