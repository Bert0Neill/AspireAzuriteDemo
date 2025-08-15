
using ConsoleAppServiceBusClient.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

var builder = Host.CreateApplicationBuilder(args);

builder.AddKeyedAzureServiceBusClient("DemoPoliciesQueue");

CancellationTokenSource cts = new CancellationTokenSource();
CancellationToken token = cts.Token;
builder.Services.AddKeyedSingleton("AppShutdown", cts);

builder.Services.AddHostedService<DemoPolicyBusService>();


using IHost host = builder.Build();
await host.RunAsync(token).ConfigureAwait(false);