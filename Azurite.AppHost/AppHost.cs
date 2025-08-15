var builder = DistributedApplication.CreateBuilder(args);

// add references to projects that you wish to view in Aspire orchestrator
builder.AddProject<Projects.Azurite_APIs>("azurite-APIs");
builder.AddProject<Projects.Azurite_BlazorWasmApp>("azurite-BlazorWasmApp");

var serviceBus = builder.AddAzureServiceBus("DemoAzuriteServiceBus").RunAsEmulator().AddServiceBusQueue("DemoPoliciesQueue");

builder.AddProject<Projects.ConsoleAppServiceBusClient>("azurite-ConsoleAppServiceBusClient").WithReference(serviceBus)
    .WaitFor(serviceBus);

builder.Build().Run();
