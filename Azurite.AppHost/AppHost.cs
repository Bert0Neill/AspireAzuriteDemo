using Microsoft.Azure.SignalR;

var builder = DistributedApplication.CreateBuilder(args);


// Local Azure SignalR emulator
var signalR = builder.AddConnectionString(
    "AzureSignalR",
    "Endpoint=http://localhost;Port=8888;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH;Version=1.0;"
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


//// Add Azure Service Bus and configure to run as emulator
//var serviceBus = builder.AddAzureServiceBus("myservicebus")
//                        .RunAsEmulator(emulator =>
//                        {
//                            emulator.WithConfigurationFile(@"config\servicebus-config.json");
//                        });

//// Add Service Bus queue or topic
//serviceBus.AddServiceBusQueue("myqueue");



//// Service Bus Emulator container
//var sbEmu = builder.AddContainer("servicebus-emulator", "mcr.microsoft.com/azure-messaging/service-bus-emulator:latest")
//    .WithEnvironment("ACCEPT_EULA", "Y")
//    .WithEnvironment("MSSQL_SA_PASSWORD", "YourStrong!Pass123")
//    .WithEndpoint(name: "sb", targetPort: 5672, isProxied: false) // AMQP
//    .WithBindMount("./config.json", "/ServiceBus_Emulator/config.json"); // your queues/topics config

// Minimal API service
// add references to projects that you wish to view in Aspire orchestrator
//builder.AddProject<Projects.Azurite_APIs>("azurite-APIs")
//    .WithEnvironment("ServiceBus__ConnectionString",
//        "Endpoint=sb://servicebus-emulator/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=DummyKey;UseDevelopmentEmulator=true;");


//var serviceBus = builder.AddAzureServiceBus("DemoAzuriteServiceBus").RunAsEmulator().AddServiceBusQueue("DemoPoliciesQueue");
//builder.AddProject<Projects.ConsoleAppServiceBusClient>("azurite-ConsoleAppServiceBusClient").WithReference(serviceBus).WaitFor(serviceBus);

builder.Build().Run();
