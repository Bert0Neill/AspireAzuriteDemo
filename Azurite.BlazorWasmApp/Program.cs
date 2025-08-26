//using Microsoft.AspNetCore.Components.Web;
//using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
//using Azurite.BlazorWasmApp;

//var builder = WebAssemblyHostBuilder.CreateDefault(args);
//builder.RootComponents.Add<App>("#app");
//builder.RootComponents.Add<HeadOutlet>("head::after");

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//await builder.Build().RunAsync();

using Azurite.BlazorWasmApp;
using Azurite.BlazorWasmApp.Services;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");

builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });
builder.Services.AddScoped<SignalRService>();

//// Connect to SignalR
//builder.Services.AddSingleton(sp =>
//{
//    var hubConnection = new HubConnectionBuilder()
//        .WithUrl("https://localhost:5001/chatHub") // your Web API endpoint
//        .WithAutomaticReconnect()
//        .Build();

//    hubConnection.StartAsync(); // start immediately

//    return hubConnection;
//});

await builder.Build().RunAsync();

