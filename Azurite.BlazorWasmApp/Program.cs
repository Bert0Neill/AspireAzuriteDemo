using Azurite.BlazorWasmApp;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.AspNetCore.SignalR.Client;
using Microsoft.AspNetCore.SignalR;

var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");

// Set the client URL in code
builder.Services.AddScoped(sp => new HttpClient
{
    BaseAddress = new Uri("http://localhost:5001/")
});


//// Update this URL to match your server's actual port
//builder.Services.AddScoped(sp => new HttpClient
//{
//    BaseAddress = new Uri("https://localhost:7201") // Check your server's launchSettings.json
//});

//builder.Services.AddScoped(sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) });

//// Register HubConnection
builder.Services.AddSingleton(provider =>
{
    var nav = provider.GetRequiredService<NavigationManager>();
    return new HubConnectionBuilder()
        //.WithUrl($"{nav.BaseUri}hubs/chat") // uses same origin as Blazor app
        //.WithUrl("http://localhost:5000/hubs/chat")
        .WithUrl("https://localhost:7240/hubs/chat")

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

