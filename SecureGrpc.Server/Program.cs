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
                .MinimumLevel.Override("Microsoft", LogEventLevel.Verbose)
                .MinimumLevel.Verbose()
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
                    var cert = new X509Certificate2(Path.Combine("Certs/server2.pfx"), "1111");
                    webBuilder.UseStartup<Startup>()
                    .ConfigureKestrel(options =>
                    {
                        options.Limits.MinRequestBodyDataRate = null;
                        options.ListenLocalhost(50051, listenOptions =>
                        {
                            listenOptions.UseHttps(cert);
                            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                            listenOptions.UseConnectionLogging();

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
                        .MinimumLevel.Override("Microsoft", LogEventLevel.Verbose)
                        .MinimumLevel.Verbose()
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
