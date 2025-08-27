using Azurite.SignalR.Hubs;
using Azurite.SignalR.Services;
using Microsoft.AspNetCore.SignalR;


var builder = WebApplication.CreateBuilder(args);


// With this:
builder.Logging.ClearProviders();
builder.Logging.AddConsole();

// Add Azure SignalR emulator
builder.Services.AddSignalR().AddAzureSignalR("Endpoint=http://localhost:8888;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH;Version=1.0;");
    

//builder.Services.AddSignalR().AddAzureSignalR(); // no parameters for emulator


// Add services
builder.Services.AddControllers();

//builder.Services.AddSignalR().AddAzureSignalR(options =>
//    {
//        // Use emulator connection string
//        options.ConnectionString = "Endpoint=http://localhost:8888;AccessKey=ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789ABCDEFGH;Version=1.0;";
//    });





//builder.Services.AddCors(options =>
//{
//    options.AddDefaultPolicy(builder =>
//    {
//        builder.WithOrigins("https://localhost:5001", "http://localhost:5000", "https://localhost:7154", "https://localhost:7207", "https://localhost:7201")
//               .AllowAnyHeader()
//               .AllowAnyMethod()
//               .AllowCredentials();
//    });
//});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(builder =>
    {
        builder.WithOrigins("https://localhost:5001", "http://localhost:5000",
                           "https://localhost:7154", "https://localhost:7207",
                           "https://localhost:7201", "https://localhost:7240")
               .AllowAnyHeader()
               .AllowAnyMethod()
               .AllowCredentials();
    });
});

// Add hosted service for periodic messages
builder.Services.AddHostedService<MessageBroadcastService>();


var app = builder.Build();

// Configure pipeline
app.UseCors();
app.UseRouting();
app.MapControllers();

//app.MapHub<ChatHub>("/chatHub");
app.MapHub<ChatHub>("/hubs/chat"); // 👈 must match client URL
app.Run();

//public class ChatHub : Hub { }

