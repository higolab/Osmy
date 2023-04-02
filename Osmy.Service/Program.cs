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
                    // MEMO:HttpProtocols.Http2�ɂ���Ə�肭�ʐM�ł��Ȃ�����
                    listenOptions.Protocols = HttpProtocols.Http1AndHttp2;
                });
            });

            // �Ǝ㐫�f�f�ƃ`�F�b�N�T���̌��؂̒�����s�T�[�r�X��o�^
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