using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IdentityServer4.AccessTokenValidation;
using System.Security.Cryptography.X509Certificates;
using Microsoft.AspNetCore.Authentication.Certificate;

namespace SecureGrpc.Server
{
    public class Startup
    {
        private string stsServer = "https://localhost:44352";

        public void ConfigureServices(IServiceCollection services)
        {
            services.AddAuthorization(options =>
            {
                options.AddPolicy("protectedScope", policy =>
                 { policy.RequireClaim("scope", "grpc_protected_scope"); });
            });
            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
                .AddCertificate(options =>
                {
                    // Not recommended in production environments. The example is using a self-signed test certificate.
                    options.RevocationMode = X509RevocationMode.NoCheck;
                    options.AllowedCertificateTypes = CertificateTypes.All;
                })
                .AddIdentityServerAuthentication(options =>
                {
                    options.Authority = stsServer;
                    options.ApiName = "ProtectedGrpc";
                    options.ApiSecret = "grpc_protected_secret";
                    options.RequireHttpsMetadata = false;
                });

            services.AddGrpc(options =>
            {
                options.EnableDetailedErrors = true;
            });

            services.AddMvc()
               .AddNewtonsoftJson();

            services.AddHttpContextAccessor();
            services.AddSingleton<ServerGrpcSubscribers>();

        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();
            app.UseHttpsRedirection();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();
            app.UseCors();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapGrpcService<GreeterService>().RequireAuthorization("protectedScope");
                endpoints.MapGrpcService<DuplexService>().RequireAuthorization("protectedScope");
                endpoints.MapRazorPages();
            });

        }
    }
}
