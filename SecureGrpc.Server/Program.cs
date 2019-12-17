using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Security.Cryptography.X509Certificates;
using System.IO;
using Serilog;
using Serilog.Events;
using System;
using Serilog.Sinks.SystemConsole.Themes;

namespace SecureGrpc.Server
{
    public class Program
    {
        public static int Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .MinimumLevel.Override("Microsoft", LogEventLevel.Information)
                .Enrich.FromLogContext()
                .WriteTo.Console()
                .CreateLogger();

            try
            {
                Log.Information("Starting web host");
                CreateHostBuilder(args).Build().Run();
                return 0;
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Host terminated unexpectedly");
                return 1;
            }
            finally
            {
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                    .ConfigureKestrel(options =>
                    {
                        options.Limits.MinRequestBodyDataRate = null;
                        options.ListenLocalhost(50051, listenOptions =>
                        {
                            listenOptions.UseHttps("Certs\\server1.pfx", "1111");
                            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                        });
                        //var cert = new X509Certificate2(Path.Combine("server.pfx"), "1111");
                        //options.ConfigureHttpsDefaults(o =>
                        //{
                        //    o.ServerCertificate = cert;
                        //    o.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                        //});

                    })
                    .UseSerilog((hostingContext, loggerConfiguration) => loggerConfiguration
                        .ReadFrom.Configuration(hostingContext.Configuration)
                        .Enrich.FromLogContext()
                        .WriteTo.File("../_GrpcServerLogs.txt")
                        .WriteTo.Console(theme: AnsiConsoleTheme.Code)
                    );
                    //.ConfigureKestrel(options =>
                    // {
                    //     var cert = new X509Certificate2(Path.Combine("sts_dev_cert.pfx"), "1234");
                    //     options.ConfigureHttpsDefaults(o =>
                    //     {
                    //         o.ServerCertificate = cert;
                    //         o.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                    //     });
                    // })
                });
    }
}
