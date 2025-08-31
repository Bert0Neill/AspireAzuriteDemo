using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.SignalR.Management;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Extensions.SignalRService;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System.Security.Claims;
using System.Text;
using System.Text.RegularExpressions;

namespace Azurite.Fnx_MonitorServicebusQueue
{
    public class SignalR_Fnxs
    {
        private readonly ILogger<SignalR_Fnxs> _logger;

        public SignalR_Fnxs(ILogger<SignalR_Fnxs> logger)
        {
            _logger = logger;
        }

        [Function("SignalR_Fnxs")]
        public IActionResult Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
        {
            _logger.LogInformation("C# HTTP trigger function processed a request.");
            return new OkObjectResult("Welcome to Azure Functions!");
        }


        [FunctionName("negotiate")]
        public static SignalRConnectionInfo GetSignalRInfo(
           [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req,
           [SignalRConnectionInfo(HubName = "chat", UserId = "{headers.x-ms-signalr-userid}")] SignalRConnectionInfo connectionInfo)
        {
            return connectionInfo;
        }

        //// Each function must have a unique name, you can uncomment this one and comment the above GetSignalRInfo() function to have a try.
        //// This "negotiate" function shows how to utilize ServiceManager to generate access token and client url to Azure SignalR service.
        //[FunctionName("negotiate")]
        //public static SignalRConnectionInfo GetSignalRInfo(
        //    [HttpTrigger(AuthorizationLevel.Anonymous)] HttpRequest req)
        //{
        //    var userId = req.Query["userid"];
        //    var hubName = req.Query["hubname"];
        //    var connectionInfo = new SignalRConnectionInfo();
        //    var serviceManager = StaticServiceHubContextStore.Get().ServiceManager;
        //    connectionInfo.AccessToken = serviceManager
        //        .GenerateClientAccessToken(
        //            hubName,
        //            userId,
        //            new List<Claim> { new Claim("claimType", "claimValue") });
        //    connectionInfo.Url = serviceManager.GetClientEndpoint(hubName);
        //    return connectionInfo;
        //}

        [FunctionName("broadcast")]
        public static async Task Broadcast(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            var message = new JsonSerializer().Deserialize<ChatMessage>(new JsonTextReader(new StreamReader(req.Body)));
            var serviceHubContext = await StaticServiceHubContextStore.Get().GetAsync("chat");
            await serviceHubContext.Clients.All.SendAsync("newMessage", message);
        }

        [FunctionName("messages")]
        public static Task SendMessage(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalR(HubName = "chat")] IAsyncCollector<SignalRMessage> signalRMessages)
        {
            var message = new JsonSerializer().Deserialize<ChatMessage>(new JsonTextReader(new StreamReader(req.Body)));

            return signalRMessages.AddAsync(
                new SignalRMessage
                {
                    UserId = message.Recipient,
                    GroupName = message.Groupname,
                    Target = "newMessage",
                    Arguments = new[] { message }
                });
        }

        [FunctionName("addToGroup")]
        public static Task AddToGroup(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalR(HubName = "chat")] IAsyncCollector<SignalRGroupAction> signalRGroupActions)
        {

            var message = new JsonSerializer().Deserialize<ChatMessage>(new JsonTextReader(new StreamReader(req.Body)));

            var decodedfConnectionId = GetBase64DecodedString(message.ConnectionId);

            return signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    ConnectionId = decodedfConnectionId,
                    UserId = message.Recipient,
                    GroupName = message.Groupname,
                    Action = GroupAction.Add
                });
        }

        [FunctionName("removeFromGroup")]
        public static Task RemoveFromGroup(
            [HttpTrigger(AuthorizationLevel.Anonymous, "post")] HttpRequest req,
            [SignalR(HubName = "chat")] IAsyncCollector<SignalRGroupAction> signalRGroupActions)
        {

            var message = new JsonSerializer().Deserialize<ChatMessage>(new JsonTextReader(new StreamReader(req.Body)));

            var decodedfConnectionId = GetBase64DecodedString(message.ConnectionId);

            return signalRGroupActions.AddAsync(
                new SignalRGroupAction
                {
                    ConnectionId = message.ConnectionId,
                    UserId = message.Recipient,
                    GroupName = message.Groupname,
                    Action = GroupAction.Remove
                });
        }

        private static string GetBase64EncodedString(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            return Convert.ToBase64String(Encoding.UTF8.GetBytes(source));
        }

        private static string GetBase64DecodedString(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return source;
            }

            return Encoding.UTF8.GetString(Convert.FromBase64String(source));
        }

        public class ChatMessage
        {
            public string Sender { get; set; }
            public string Text { get; set; }
            public string Groupname { get; set; }
            public string Recipient { get; set; }
            public string ConnectionId { get; set; }
            public bool IsPrivate { get; set; }
        }

        public class SignalREvent
        {
            public DateTime Timestamp { get; set; }
            public string HubName { get; set; }
            public string ConnectionId { get; set; }
            public string UserId { get; set; }
        }

    }
}
