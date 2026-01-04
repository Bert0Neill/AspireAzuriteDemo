using Azurite.BlazorWasmApp;
using Azurite.BlazorWasmApp.Infrastructure;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Register HttpClient for API service
// In Blazor WASM, we register HttpClient directly
builder.Services.AddScoped(sp =>
{
    var httpClient = new HttpClient();
    // Use Aspire service discovery or fallback to localhost
    var apiUrl = builder.Configuration["Services:Azurite-Api"] ?? "http://localhost:5001";
    httpClient.BaseAddress = new Uri(apiUrl);
    return httpClient;
});

builder.Services.AddScoped<ApiService>();


//// Update this URL to match your server's actual port
//builder.Services.AddScoped(sp => new HttpClient
//{
//    BaseAddress = new Uri("https://localhost:7201") // Check your server's launchSettings.json
//});

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

// Register HubConnection - connect to API project's SignalR hub
builder.Services.AddSingleton(provider =>
{
    var nav = provider.GetRequiredService<NavigationManager>();
    // Use Aspire service discovery or fallback to localhost
    var apiUrl = builder.Configuration["Services:Azurite-Api"] ?? "http://localhost:5001";
    var hubUrl = $"{apiUrl}/hubs/chat";
    
    return new HubConnectionBuilder()
        .WithUrl(hubUrl)
        .WithAutomaticReconnect()
        .Build();
});


//// Register HubConnection
//builder.Services.AddSingleton(provider =>
//{
//    // Connect directly to the server's SignalR endpoint
//    return new HubConnectionBuilder()
//        //.WithUrl("http://localhost:5000/hubs/chat")
//        .WithUrl("https://localhost:5001/hubs/chat")
//        .WithAutomaticReconnect()
//        .Build();
//});





await builder.Build().RunAsync();

