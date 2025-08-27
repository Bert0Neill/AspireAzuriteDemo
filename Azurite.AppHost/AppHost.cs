using Aspire.Azure.Messaging.ServiceBus;
using Aspire.Hosting;
using Microsoft.Azure.SignalR;
using Microsoft.Extensions.Configuration;
using Scalar.Aspire;
using System.ComponentModel;
using Aspire.Hosting.Azure;

var builder = DistributedApplication.CreateBuilder(args);

var scalar = builder.AddScalarApiReference();

/**************************************************
 *          Ensure Docker is running!!!           *
 **************************************************/

//// Add caching with Redis
//var cache = builder.AddRedis("cache")
//    .WithLifetime(ContainerLifetime.Persistent)
//    .WithRedisInsight();

//#region SQL Server
//// Add SQL Server 
////var passwordParameter = builder.AddParameter("password", "P@ssw0rd");
////var sql = builder
////    .AddSqlServer("sql", port: 58349, password: passwordParameter)
////    .WithLifetime(ContainerLifetime.Persistent);
//////.AddDatabase("servicebus-db"); // Database for Service Bus emulator
//#endregion

//#region Load environment settings
//// load setting form appsetting file (DEV\UAT\PROD etc.)
////builder.Configuration.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
////                     .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json", optional: true, reloadOnChange: true)
////                     .AddEnvironmentVariables();
//#endregion

//// Local Azure Service Bus emulator
//var serviceBus = builder
//    .AddAzureServiceBus("sbInsurancePolicies")
//#if DEBUG
//    //.RunAsEmulator(c => c.WithLifetime(ContainerLifetime.Persistent));    
//    .RunAsEmulator();
//#endif
//serviceBus.AddServiceBusQueue("propertyContent");

//// Reference Azurite (Blob + Queue + Table)
//var driveRoot = Path.GetPathRoot(Environment.CurrentDirectory);
//var azuriteDataPath = Path.Combine(driveRoot!, "Azurite"); // Combine it with "Azurite"
//if (!Directory.Exists(azuriteDataPath))
//{
//    Directory.CreateDirectory(azuriteDataPath);
//}

//var azurite = builder.AddContainer("Azurite-Storage-Emulator", "mcr.microsoft.com/azure-storage/azurite")
//    .WithBindMount(azuriteDataPath, "/Azurite-Data")
//    .WithEndpoint(10000, 10000, name: "blob")   // Blob service
//    .WithEndpoint(10001, 10001, name: "queue")  // Queue service
//    .WithEndpoint(10002, 10002, name: "table"); // Table service

//// Build ReferenceExpression dynamically using container endpoints
//var azuriteConnExpr = ReferenceExpression.Create($@"
//        DefaultEndpointsProtocol=http;
//        AccountName=devstoreaccount1;
//        AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;
//        BlobEndpoint={azurite.GetEndpoint("blob")}/devstoreaccount1;
//        QueueEndpoint={azurite.GetEndpoint("queue")}/devstoreaccount1;
//        TableEndpoint={azurite.GetEndpoint("table")}/devstoreaccount1;
//        ");

//// Add connection string resource using the ReferenceExpression
//var azuriteConn = builder.AddConnectionString("Azurite-Storage-Conns", azuriteConnExpr);

//// Reference your Fnx project
//var fnxQ = builder.AddProject<Projects.Azurite_Fnx_MonitorServicebusQueue>("Azurite-Fnx-Q")
//               .WithReference(serviceBus)               
//               .WithReference(azuriteConn) // inject Azurite connection string
//               .WaitFor(azurite)
//               .WaitFor(serviceBus)
//               ;

//// Reference your Web API project
//var api = builder.AddProject<Projects.Azurite_APIs>("Azurite-Api")        
//           .WithReference(serviceBus)
//           .WaitFor(serviceBus)
//           ;

//// Add Azure SignalR emulator
//var signalrEmulator = builder.AddAzureSignalR("Emulator-SignalR", AzureSignalRServiceMode.Serverless)
//    .RunAsEmulator();


// Reference your SignalR project
var signalR = builder.AddProject<Projects.Azurite_SignalR>("Azurite-SignalR")    
                //.WithReference(fnxQ) // fnx retrieves message from Queue
                //.WithReference(api) // api pushes message onto Queue
                //.WaitFor(fnxQ)
                //.WaitFor(signalrEmulator)
                ;

// Reference your Blazor WASM project
var blazor = builder.AddProject<Projects.Azurite_BlazorWasmApp>("Azurite-BlazorWasmApp")
               .WithReference(signalR)
               //.WaitFor(serviceBus)
               //.WaitFor(cache)
               ;

builder.Build().Run();
