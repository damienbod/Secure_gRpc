using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Server.Kestrel.Https;
using System.Security.Cryptography.X509Certificates;
using System.IO;

namespace SecureGrpc.Server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
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
                            listenOptions.UseHttps("server.pfx", "1111");
                            listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                        });
                        var cert = new X509Certificate2(Path.Combine("server.pfx"), "1111");
                        options.ConfigureHttpsDefaults(o =>
                        {
                            o.ServerCertificate = cert;
                            o.ClientCertificateMode = ClientCertificateMode.RequireCertificate;
                        });

                    });
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
