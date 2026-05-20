using FlowForge.API.Authentication;
using FlowForge.API.BackgroundServices;
using FlowForge.API.Behaviors;
using FlowForge.API.Middlewares;
using FlowForge.Application;
using FlowForge.Infrastructure;
using FlowForge.Infrastructure.Authentication;
using FlowForge.Persistence;
using MediatR;
using Serilog;

namespace FlowForge.API
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

            builder.Services.AddPersistenceServices(builder.Configuration);
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            //Serilog iþlemleri
            builder.Host.UseSerilog((context, configuration) =>
                configuration.ReadFrom.Configuration(context.Configuration));

            //Authentication handler DI Registration
            builder.Services
                .AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme;
                    options.DefaultChallengeScheme = ApiKeyAuthenticationDefaults.AuthenticationScheme;
                })
                .AddScheme<ApiKeyAuthenticationOptions, ApiKeyAuthenticationHandler>(
                ApiKeyAuthenticationDefaults.AuthenticationScheme,
                options => { });

            builder.Services.AddHostedService<DeliveryProcessorWorker>();
            builder.Services.AddHostedService<DeliveryRecoveryWorker>();

            builder.Services.AddScoped(typeof(IPipelineBehavior<,>),
                typeof(LoggingPipelineBehavior<,>));

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseMiddleware<CorrelationIdMiddleware>();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}