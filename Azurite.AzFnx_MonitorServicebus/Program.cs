using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.SignalR.Management;

var serviceManager = new ServiceManagerBuilder()
    .WithOptions(o =>
    {
        o.ConnectionString = Environment.GetEnvironmentVariable("AzureSignalRConnectionString");
    })
    .BuildServiceManager();


var host = new HostBuilder()
    .ConfigureFunctionsWebApplication()
    .ConfigureServices(services => {
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();
        
        // Register Service Bus extension for isolated worker model
        services.Configure<WorkerOptions>(options =>
        {
            options.EnableUserCodeException = true;
        });
    })
    .Build();

host.Run();