using Azurite.SignalR.Classes;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;

var builder = WebApplication.CreateBuilder(args);

// Allow Blazor client origin
builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins("https://localhost:7207") // Blazor WASM URL
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials(); // Required for SignalR
    });
});


// Add SignalR
builder.Services.AddSignalR()
    .AddAzureSignalR(options =>
    {
        // Point to emulator instance
        options.ConnectionString = "Endpoint=http://localhost;Port=8888;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH;Version=1.0;";
    });

builder.Services.AddHostedService<TimedMessageService>();

var app = builder.Build();

app.UseCors();

app.MapHub<ChatHub>("/chatHub");
app.Run();

public class ChatHub : Hub { }

