using Aspire.Hosting;
using Microsoft.Azure.SignalR;
using Aspire.Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Configuration;
using Scalar.Aspire;

var builder = DistributedApplication.CreateBuilder(args);

var scalar = builder.AddScalarApiReference();

// Add caching with Redis
var cache = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRedisInsight();

// Add SQL Server 
//var passwordParameter = builder.AddParameter("password", "P@ssw0rd");
//var sql = builder
//    .AddSqlServer("sql", port: 58349, password: passwordParameter)
//    .WithLifetime(ContainerLifetime.Persistent);
////.AddDatabase("servicebus-db"); // Database for Service Bus emulator

// load setting form appsetting file (DEV\UAT\PROD etc.)
//builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
//                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
//                     .AddEnvironmentVariables();


// Local Azure Service Bus emulator
var serviceBus = builder
    .AddAzureServiceBus("sbemulat")
#if DEBUG
    .RunAsEmulator(c => c.WithLifetime(ContainerLifetime.Persistent));
    //.RunAsEmulator();
#endif
serviceBus.AddServiceBusQueue("insurancePolicies");
//serviceBus.AddServiceBusTopic("propertyContent");
//serviceBus.AddServiceBusTopic("propertyStructure");

#region Hide
//var sbEmu = builder.AddContainer("servicebus-emulator", "mcr.microsoft.com/azure-messaging/service-bus-emulator")
//    .WithEnvironment("ACCEPT_EULA", "Y")
//    .WithEnvironment("MSSQL_SA_PASSWORD", "YourStrong!Pass123")
//    .WithEndpoint(name: "sb", targetPort: 5672, isProxied: false) // AMQP
//    .WithBindMount(configPath, "/ServiceBus_Emulator/config.json"); // your queues/topics config

//// Local Azure SignalR emulator
//var signalR = builder.AddConnectionString(
//    "AzureSignalR",
//    "Endpoint=http://127.0.0.1:8888;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH"
//);
#endregion

// Reference your Fnx project
var fnxQ = builder.AddProject<Projects.Azurite_Fnx_MonitorServicebusQueue>("Azurite-Fnx-Q")
               .WithReference(serviceBus)
               .WaitFor(serviceBus);

// Reference your SignalR project
var signalR = builder.AddProject<Projects.Azurite_SignalR>("Azurite-SignalR")
                .WithReference(fnxQ)
                .WaitFor(fnxQ);


// Reference your Blazor WASM project
var blazor = builder.AddProject<Projects.Azurite_BlazorWasmApp>("Azurite-BlazorWasmApp")
               .WaitFor(serviceBus)
               .WaitFor(cache);

// Reference your Web API project
var api = builder.AddProject<Projects.Azurite_APIs>("Azurite-Api")        
           .WithReference(serviceBus)
           .WaitFor(serviceBus);

builder.Build().Run();
