using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using RigolStream.Api.Devices;
using RigolStream.Api.Infrastructure;

var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices((context, services) =>
    {
        services.Configure<OscilloscopeOptions>(
            context.Configuration.GetSection(OscilloscopeOptions.SectionName));

        // One scope instance shared across requests (the simulator keeps state).
        services.AddSingleton<IOscilloscope>(sp =>
            OscilloscopeFactory.Create(sp.GetRequiredService<Microsoft.Extensions.Options.IOptions<OscilloscopeOptions>>()));

        services.AddSingleton<SetupStore>();

        // Match the hand-written SSE serialization (camelCase, string enums).
        services.Configure<JsonOptions>(o =>
        {
            o.JsonSerializerOptions.PropertyNamingPolicy = ApiJson.Options.PropertyNamingPolicy;
            o.JsonSerializerOptions.DefaultIgnoreCondition = ApiJson.Options.DefaultIgnoreCondition;
            o.JsonSerializerOptions.Converters.Add(
                new JsonStringEnumConverter(System.Text.Json.JsonNamingPolicy.CamelCase));
        });
    })
    .Build();

host.Run();
