using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualBasic;
using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;

namespace aspdotnet_baza
{
    public class Program {
        public static void Main(string[] args) {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddControllers();
            builder.Configuration.AddEnvironmentVariables().Build();
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();
            builder.Services.AddSingleton<IMemoryCheck, MemoryChecker>();
            builder.Services.AddSingleton<IEmailSender, BegetSMTPEmailSender>(
                serviceProvider => new BegetSMTPEmailSender(LoggerFactory.Create(builder => {
                    builder.AddSimpleConsole(i =>
                        i.ColorBehavior = Microsoft.Extensions.Logging.Console.LoggerColorBehavior.Enabled);
                }).CreateLogger("Program")));
            builder.Services.AddHostedService(serviceProvider =>
                new BackgroundEmailMemoryService(
                    serviceProvider.GetService<IEmailSender>() ?? throw new ArgumentNullException("emailSender"),
                TimeSpan.FromSeconds(10),
                serviceProvider.GetService<IMemoryCheck>() ?? throw new ArgumentNullException("memoryCheck"),
                builder.Configuration));
            var app = builder.Build();
            app.UseSwagger();
            app.UseSwaggerUI(options => {
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "v1");
                options.RoutePrefix = string.Empty;
            });
            app.MapGet("/sendemails",
                async (
                    IEmailSender emailSender,
                    ILogger<Program> logger,
                    IConfiguration configuration,
                    CancellationToken token) => {
                    if (emailSender.IsConnected == false)
                        await emailSender.ConnectAsync(
                            configuration["BegetSMTPLog_inData:Host"] ?? throw new ArgumentException("Host"),
                            int.Parse(configuration["BegetSMTPLog_inData:Port"] ?? throw new ArgumentException("Port")),
                            false,
                            token);
                    if (emailSender.IsAuthenticated == false)
                        await emailSender.AuthenticateAsync(
                            configuration["BegetSMTPLog_inData:Login"] ?? throw new ArgumentException("Login"),
                            configuration["BegetSMTPLog_inData:Password"] ?? throw new ArgumentException("Password"),
                            token);
                    for (int i = 0; i < 10; i++) {
                        logger.LogInformation($"sending {i + 1} email...");
                        var stopwatch = Stopwatch.StartNew();
                        await emailSender.SendEmailAsync(
                            "anonymous",
                            "asp2022pd011@rodion-m.ru",
                            "AG", "desu99@bk.ru",
                            "Memory",
                            "Sent",
                            token);
                        stopwatch.Stop();
                        logger.LogInformation($"email {i + 1} sent in {stopwatch.ElapsedMilliseconds} ms");
                    }});
            app.Run();
        }
    }
}
