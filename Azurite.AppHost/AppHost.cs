var builder = DistributedApplication.CreateBuilder(args);


/**************************************************
 *          Ensure Docker is running!!!           *
 **************************************************/

// Add caching with Redis
var cache = builder.AddRedis("cache")
    .WithLifetime(ContainerLifetime.Persistent)
    .WithRedisInsight();

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

// Local Azure Service Bus emulator
var serviceBus = builder
    .AddAzureServiceBus("sbInsurancePolicies")
#if DEBUG
    //.RunAsEmulator(c => c.WithLifetime(ContainerLifetime.Persistent));    
    .RunAsEmulator();
#endif
serviceBus.AddServiceBusQueue("propertyContent");

// Reference Azurite (Blob + Queue + Table)
var azuriteDataPath = Path.Combine(Environment.CurrentDirectory, "Emulators", "Azurite-data");
if (!Directory.Exists(azuriteDataPath))
{
    Directory.CreateDirectory(azuriteDataPath);
}

var azurite = builder.AddContainer("Azurite-Storage-Emulator", "mcr.microsoft.com/azure-storage/azurite")
    .WithBindMount(azuriteDataPath, "/data")
    .WithEndpoint(10000, 10000, name: "blob")   // Blob service
    .WithEndpoint(10001, 10001, name: "queue")  // Queue service
    .WithEndpoint(10002, 10002, name: "table")  // Table service
    .WithLifetime(ContainerLifetime.Persistent);

// Build connection string dynamically using container endpoints
// Note: For Azurite, we use the standard development storage account
// The endpoints will be resolved at runtime by Aspire
var blobEndpoint = azurite.GetEndpoint("blob");
var queueEndpoint = azurite.GetEndpoint("queue");
var tableEndpoint = azurite.GetEndpoint("table");

var azuriteConnExpr = ReferenceExpression.Create($@"
        DefaultEndpointsProtocol=http;
        AccountName=devstoreaccount1;
        AccountKey=Eby8vdM02xNOcqFlqUwJPLlmEtlCDXJ1OUzFT50uSRZ6IFsuFq2UVErCz4I6tq/K1SZFPTOtr/KBHBeksoGMGw==;
        BlobEndpoint=http://{blobEndpoint}/devstoreaccount1;
        QueueEndpoint=http://{queueEndpoint}/devstoreaccount1;
        TableEndpoint=http://{tableEndpoint}/devstoreaccount1;
        ");

// Add connection string resource using the ReferenceExpression
var azuriteConn = builder.AddConnectionString("AzuriteStorage", azuriteConnExpr);

// Note: Azure Key Vault doesn't have an official emulator like Azurite.
// The VaultService uses in-memory storage for local development.
// For production, use Azure Key Vault with proper authentication.
var keyVaultConn = builder.AddConnectionString("KeyVault", "http://localhost:8080");

//// Reference your Fnx project
//var fnxQ = builder.AddProject<Projects.Azurite_Fnx_MonitorServicebusQueue>("Azurite-Fnx-Q")
//               .WithReference(serviceBus)               
//               .WithReference(azuriteConn) // inject Azurite connection string
//               .WaitFor(azurite)
//               .WaitFor(serviceBus)
//               ;

// Reference your Web API project
var api = builder.AddProject<Projects.Azurite_APIs>("Azurite-Api")
           .WithReference(serviceBus)
           .WithReference(azuriteConn)
           .WithReference(keyVaultConn)
           .WaitFor(serviceBus)
           .WaitFor(azurite)
           ;

// Add Azure SignalR emulator
var signalrEmulator = builder.AddAzureSignalR("Emulator-SignalR")
    .RunAsEmulator();

// Reference your Azure Functions project
// Azure Functions can be added as a regular project in Aspire
var fnxQ = builder.AddProject<Projects.Azurite_Fnx_MonitorServicebusQueue>("Azurite-Fnx-Q")
               .WithReference(serviceBus)
               .WithReference(azuriteConn)
               .WithReference(signalrEmulator)
               .WaitFor(azurite)
               .WaitFor(serviceBus)
               .WaitFor(signalrEmulator)
               ;

// Reference your SignalR project
var signalR = builder.AddProject<Projects.Azurite_SignalR>("Azurite-SignalR")
                .WithReference(fnxQ)
                .WithReference(api)
                .WithReference(signalrEmulator)
                .WaitFor(fnxQ)
                .WaitFor(signalrEmulator)
                ;

// Reference your Blazor WASM project
var blazor = builder.AddProject<Projects.Azurite_BlazorWasmApp>("Azurite-BlazorWasmApp")
               .WithReference(api)
               .WithReference(signalR)
               ;

builder.Build().Run();
