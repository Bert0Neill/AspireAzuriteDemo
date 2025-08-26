using Azurite.SignalR.Classes;
using Azurite.SignalR.Services;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR.Management;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddSignalR()
    .AddAzureSignalR(options =>
    {
        // Use emulator connection string
        options.ConnectionString = "Endpoint=http://localhost:8888;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH;Version=1.0;";
    });

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://localhost:5001", "http://localhost:5000")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

//// Allow Blazor client origin
//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(policy =>
//    {
//        policy.WithOrigins("https://localhost:7207") // Blazor WASM URL
//              .AllowAnyHeader()
//              .AllowAnyMethod()
//              .AllowCredentials(); // Required for SignalR
//    });
//});

//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(builder =>
//    {
//        builder.WithOrigins("https://localhost:5001", "http://localhost:5000")
//               .AllowAnyHeader()
//               .AllowAnyMethod()
//               .AllowCredentials();
//    });
//});


//// Add SignalR
//builder.Services.AddSignalR()
//    .AddAzureSignalR(options =>
//    {
//        // Point to emulator instance
//        options.ConnectionString = "Endpoint=http://localhost;Port=8888;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH;Version=1.0;";
//    });

//builder.Services.AddHostedService<TimedMessageService>();

// Add hosted service for periodic messages
builder.Services.AddHostedService<MessageBroadcastService>();

var app = builder.Build();

// Configure pipeline
app.UseCors();
app.UseRouting();

app.MapControllers();
app.MapHub<ChatHub>("/chathub");
app.Run();

public class ChatHub : Hub { }

