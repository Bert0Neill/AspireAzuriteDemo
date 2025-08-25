
using ConsoleAppServiceBusClient.Services;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Azure.Messaging.ServiceBus;

var builder = Host.CreateApplicationBuilder(args);

// Use Aspire's Azure Service Bus connection string
var serviceBusConnectionString = builder.Configuration["ConnectionStrings:AzureServiceBus"];
if (serviceBusConnectionString != null)
{
    builder.Services.AddSingleton(_ => new ServiceBusClient(serviceBusConnectionString));
}

CancellationTokenSource cts = new CancellationTokenSource();
CancellationToken token = cts.Token;
builder.Services.AddKeyedSingleton("AppShutdown", cts);

builder.Services.AddHostedService<DemoPolicyBusService>();


using IHost host = builder.Build();
await host.RunAsync(token).ConfigureAwait(false);