using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using IdentityServer4.AccessTokenValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using System.Security.Claims;

namespace Secure_gRpc
{
    public class Startup
    {
        private string stsServer = "https://localhost:44352";

        // This method gets called by the runtime. Use this method to add services to the container.
        // For more information on how to configure your application, visit https://go.microsoft.com/fwlink/?LinkID=398940
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddHttpContextAccessor();

            services.AddSingleton<ServerGrpcSubscribers>();

            services.AddAuthorization(options =>
            {
                options.AddPolicy("protectedScope", policy =>
                {
                    policy.RequireClaim("scope", "grpc_protected_scope");
                });
            });

            services.AddAuthorizationPolicyEvaluator();

            services.AddAuthentication(IdentityServerAuthenticationDefaults.AuthenticationScheme)
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
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseStaticFiles();

            app.UseRouting(routes =>
            {
                routes.MapGrpcService<GreeterService>().RequireAuthorization("protectedScope");
                routes.MapGrpcService<DuplexService>().RequireAuthorization("protectedScope");
                routes.MapRazorPages();
            });

            app.UseAuthentication();
            app.UseAuthorization();
        }
    }
}
