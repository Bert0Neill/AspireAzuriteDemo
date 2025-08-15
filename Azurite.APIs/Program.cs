using Azure.Messaging.ServiceBus;
using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

//builder.AddAzureServiceBusClient("DemoAzuriteServiceBus");
// Connection string to your Service Bus emulator or real instance
string connectionString = builder.Configuration["ServiceBus:ConnectionString"];
string queueName = builder.Configuration["ServiceBus:QueueName"];

builder.Services.AddSingleton(new ServiceBusClient(connectionString));
builder.Services.AddSingleton(sp =>
    sp.GetRequiredService<ServiceBusClient>().CreateSender(queueName));

var app = builder.Build();

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
