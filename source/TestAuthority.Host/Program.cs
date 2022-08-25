using System.Text.Json;
using System.Text.Json.Serialization;
using FluentValidation.AspNetCore;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using TestAuthority.Application;
using TestAuthority.Application.Random;
using TestAuthority.Domain;
using TestAuthority.Domain.Services;
using TestAuthority.Host.Extensions;
using TestAuthority.Host.Filters;
using TestAuthority.Infrastructure.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog(((context, services, configuration) =>
{
    configuration.ReadFrom.Configuration(context.Configuration);
    configuration.ReadFrom.Services(services);
    configuration.Enrich.FromLogContext();
    configuration.WriteTo.Console();
}));

builder.Services
    .AddControllers(x => x.Filters.Add<ValidationFilter>())
    .AddJsonOptions(ConfigureJson)
    .AddFluentValidation(config => config.RegisterValidatorsFromAssemblyContaining<Program>());

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwagger();

builder.Services.AddMediatR(typeof(Program));
builder.Services.AddSingleton<ITimeServer, TimeServer>();
builder.Services.AddSingleton<IRandomService, RandomService>();
builder.Services.AddCertificateAuthorityService();
builder.Services.AddCertificateGenerationPipeline();
builder.Services.AddCrlGenerationPipeline();

builder.Services.Configure<CrlSettings>(builder.Configuration.GetSection("CrlSettings"));
builder.Configuration.AddEnvironmentVariables();


var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI(config =>
{
    config.SwaggerEndpoint("/swagger/v1/swagger.json", "Personal Signing Center");
    config.RoutePrefix = string.Empty;
});

app.MapControllers();
app.Run();

void ConfigureJson(JsonOptions options)
{
    options.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
    options.JsonSerializerOptions.PropertyNameCaseInsensitive = true;
    options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
}
