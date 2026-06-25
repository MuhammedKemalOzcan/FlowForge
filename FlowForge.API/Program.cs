using FlowForge.API.Authentication;
using FlowForge.API.BackgroundServices;
using FlowForge.API.Behaviors;
using FlowForge.API.HealthChecks;
using FlowForge.API.Middlewares;
using FlowForge.Application;
using FlowForge.Infrastructure;
using FlowForge.Infrastructure.Authentication;
using FlowForge.Persistence;
using MediatR;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;

namespace FlowForge.API
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowDashboard", policy =>
                {
                    policy.WithOrigins("http://localhost:3000")  // Next.js frontend
                          .AllowAnyHeader()
                          .AllowAnyMethod()
                          .AllowCredentials()
                          .WithExposedHeaders("X-Correlation-Id");  // Frontend okuyabilsin
                });
            });

            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddPersistenceServices(builder.Configuration);
            builder.Services.AddApplicationServices();
            builder.Services.AddInfrastructureServices(builder.Configuration);

            //Serilog i�lemleri
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
            builder.Services.AddHostedService<DemoCleanupWorker>();

            builder.Services.AddScoped(typeof(IPipelineBehavior<,>),
                typeof(LoggingPipelineBehavior<,>));

            builder.Services.AddHealthChecks()
                .AddCheck<PostgreSqlHealthCheck>("postgresql")
                .AddCheck<RedisHealthCheck>("redis");

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowDashboard");

            app.UseMiddleware<ExceptionHandlingMiddleware>();

            app.UseHttpsRedirection();

            app.UseMiddleware<CorrelationIdMiddleware>();

            app.UseAuthorization();

            app.MapControllers();

            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                Predicate = _ => false
            });

            app.MapHealthChecks("/health/ready", new HealthCheckOptions
            {
                Predicate      = _ => true,
                ResponseWriter = WriteHealthJson
            });

            app.Run();
        }

        private static Task WriteHealthJson(HttpContext ctx, HealthReport report)
        {
            ctx.Response.ContentType = "application/json";
            return ctx.Response.WriteAsJsonAsync(new
            {
                status = report.Status.ToString(),
                checks = report.Entries.Select(e => new
                {
                    name        = e.Key,
                    status      = e.Value.Status.ToString(),
                    description = e.Value.Description
                })
            });
        }
    }
}