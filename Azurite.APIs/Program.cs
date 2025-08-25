using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.SignalR;
using Scalar.AspNetCore;
using Azurite.APIs.Hubs;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddOpenApi();

// If Aspire injected the connection string into environment or config
var azureSignalRConnectionString = builder.Configuration.GetConnectionString("AzureSignalR");

// Register SignalR
builder.Services.AddSignalR().AddAzureSignalR(azureSignalRConnectionString);

// Configure Azure SignalR
//builder.Services.AddSignalR().AddAzureSignalR(builder.Configuration.GetConnectionString("AzureSignalR"));



// Configure Azure Service Bus client using Aspire connection string
var serviceBusConnectionString = builder.Configuration["ConnectionStrings:AzureServiceBus"];
string queueName = "myqueue";
if (serviceBusConnectionString != null)
{
    builder.Services.AddSingleton(_ => new ServiceBusClient(serviceBusConnectionString));
    builder.Services.AddSingleton(sp => sp.GetRequiredService<ServiceBusClient>().CreateSender(queueName));
}


var app = builder.Build();

// Map SignalR hub
app.MapHub<MyHub>("/hub");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();

}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};


app.MapPost("/send", async (ServiceBusSender sender, MessageDto message) =>
{
    var serviceBusMessage = new ServiceBusMessage(message.Text)
    {
        ContentType = "text/plain"
    };

    await sender.SendMessageAsync(serviceBusMessage);

    return Results.Ok($"Message sent to queue '{queueName}'");

    //// push forecast to Service Bus queue
    //ServiceBusSender _sender = busClient.CreateSender("DemoPoliciesQueue");
    //ServiceBusMessage message = new(forecast.ToString());
    //await _sender.SendMessageAsync(message);

});

app.MapGet("/weatherforecast", async (ServiceBusClient busClient) =>
{
    var forecast =  Enumerable.Range(1, 5).Select(index =>
        new WeatherForecast
        (
            DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
            Random.Shared.Next(-20, 55),
            summaries[Random.Shared.Next(summaries.Length)]
        ))
        .ToArray();



    return forecast;
})
.WithName("GetWeatherForecast");

app.Run();

record MessageDto(string Text);

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}
