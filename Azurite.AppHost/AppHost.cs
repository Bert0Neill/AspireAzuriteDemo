using Microsoft.Azure.SignalR;

var builder = DistributedApplication.CreateBuilder(args);


// Local Azure SignalR emulator
var signalR = builder.AddConnectionString(
    "AzureSignalR",
    "Endpoint=http://127.0.0.1:8888;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH"
);


// Local Azure Service Bus emulator
var serviceBus = builder.AddConnectionString(
    "AzureServiceBus",
    "Endpoint=sb://localhost/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=Eby8vdM02xNOcqFeqCg=="
);



// Reference your Web API project
builder.AddProject<Projects.Azurite_APIs>("webapi")
       .WithReference(signalR)
       .WithReference(serviceBus);

// Reference your Blazor WASM project
builder.AddProject<Projects.Azurite_BlazorWasmApp>("azurite-BlazorWasmApp");

// Reference your Console App Service Bus Client project
builder.AddProject<Projects.ConsoleAppServiceBusClient>("azurite-ConsoleAppServiceBusClient")
       .WithReference(serviceBus);

builder.Build().Run();
