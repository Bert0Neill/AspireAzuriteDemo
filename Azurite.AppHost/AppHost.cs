using Aspire.Hosting;

var builder = DistributedApplication.CreateBuilder(args);

// Service Bus Emulator container
var sbEmu = builder.AddContainer("servicebus-emulator", "mcr.microsoft.com/azure-messaging/service-bus-emulator:latest")
    .WithEnvironment("ACCEPT_EULA", "Y")
    .WithEnvironment("MSSQL_SA_PASSWORD", "YourStrong!Pass123")
    .WithEndpoint(name: "sb", targetPort: 5672, isProxied: false) // AMQP
    .WithBindMount("./config.json", "/ServiceBus_Emulator/config.json"); // your queues/topics config

// Minimal API service
// add references to projects that you wish to view in Aspire orchestrator
builder.AddProject<Projects.Azurite_APIs>("azurite-APIs")
    .WithEnvironment("ServiceBus__ConnectionString",
        "Endpoint=sb://servicebus-emulator/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=DummyKey;UseDevelopmentEmulator=true;");

builder.AddProject<Projects.Azurite_BlazorWasmApp>("azurite-BlazorWasmApp");

//var serviceBus = builder.AddAzureServiceBus("DemoAzuriteServiceBus").RunAsEmulator().AddServiceBusQueue("DemoPoliciesQueue");
//builder.AddProject<Projects.ConsoleAppServiceBusClient>("azurite-ConsoleAppServiceBusClient").WithReference(serviceBus).WaitFor(serviceBus);

builder.Build().Run();
