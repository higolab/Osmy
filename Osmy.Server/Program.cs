using Microsoft.AspNetCore.Server.Kestrel.Core;
using Osmy.Core.Configuration;
using Osmy.Server.Data.Sbom.Spdx;
using Osmy.Server.Services;
using System.Diagnostics;

namespace Osmy.Server
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            string socketPath = DefaultServerConfig.UnixSocketPath;
            builder.WebHost.ConfigureKestrel(options =>
            {
                if (File.Exists(socketPath))
                {
                    File.Delete(socketPath);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(socketPath)!);
                }

                options.ListenUnixSocket(socketPath, listenOptions =>
                {
                    // MEMO:HttpProtocols.Http2にすると上手く通信できなかった
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            });

            // 脆弱性診断とチェックサムの検証の定期実行サービスを登録
            builder.Services.AddSingleton<VulnerabilityScanService>();
            builder.Services.AddHostedService(p => p.GetRequiredService<VulnerabilityScanService>());
            builder.Services.AddSingleton<ChecksumVerificationService>();
            builder.Services.AddHostedService(p => p.GetRequiredService<ChecksumVerificationService>());

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            await app.StartAsync();

            if (Environment.OSVersion.Platform == PlatformID.Unix && File.Exists(DefaultServerConfig.UnixSocketPath))
            {
                Process.Start("chmod", $"a+w {DefaultServerConfig.UnixSocketPath}");
            }

            if (!await SpdxConverter.FetchConverterAsync())
            {
                Console.Error.WriteLine("Failed to fetch spdx/tools-java");
                Environment.Exit(1);
            }

            await app.WaitForShutdownAsync();
        }
    }
}