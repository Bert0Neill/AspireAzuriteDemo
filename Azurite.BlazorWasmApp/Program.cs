//using Microsoft.AspNetCore.Components.Web;
//using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
//using Azurite.BlazorWasmApp;

//var builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder.RootComponents.Add<App>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//await builder.Build().RunAsync();

using Azurite.BlazorWasmApp;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

// Connect to SignalR
builder.Services.AddSingleton(sp =>
{
    var hubConnection = new HubConnectionBuilder()
        .WithUrl("http://localhost:5000/hub") // your Web API endpoint
        .WithAutomaticReconnect()
        .Build();

    hubConnection.StartAsync(); // start immediately

    return hubConnection;
});

await builder.Build().RunAsync();

