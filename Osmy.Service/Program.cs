using Microsoft.AspNetCore.Server.Kestrel.Core;
using Osmy.Service.Services;

namespace Osmy.Service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            string SocketPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Osmy", "osmy.service.sock");
            builder.WebHost.ConfigureKestrel(options =>
            {
                if (File.Exists(SocketPath))
                {
                    File.Delete(SocketPath);
                }
                else
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(SocketPath)!);
                }

                options.ListenUnixSocket(SocketPath, listenOptions =>
                {
                    // MEMO:HttpProtocols.Http2にすると上手く通信できなかった
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            });

            // 脆弱性診断とチェックサムの検証の定期実行サービスを登録
            builder.Services.AddHostedService<VulnerabilityScanService>();
            builder.Services.AddHostedService<ChecksumVerificationService>();

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseAuthorization();


            app.MapControllers();

            app.Run();
        }
    }
}