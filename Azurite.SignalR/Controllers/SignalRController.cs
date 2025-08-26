namespace Azurite.SignalR.Controllers
{
    using Microsoft.AspNetCore.Mvc;
    using Microsoft.AspNetCore.SignalR;
    using Microsoft.Azure.SignalR;
    using Microsoft.Azure.SignalR.Management;


    [ApiController]
    [Route("api/[controller]")]
    public class SignalRController : ControllerBase
    {
        private readonly IServiceManager _serviceManager;

        public SignalRController(IServiceManager serviceManager)
        {
            _serviceManager = serviceManager;
        }

        [HttpPost("negotiate")]
        public async Task<IActionResult> Negotiate([FromBody] NegotiateRequest request)
        {
            try
            {
                // Get the client endpoint
                var clientEndpoint = _serviceManager.GetClientEndpoint("chathub");

                // Generate access token for the client
                var accessToken = _serviceManager.GenerateClientAccessToken("chathub", request.UserId);

                var response = new NegotiateResponse
                {
                    Url = clientEndpoint,
                    AccessToken = accessToken
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest($"Negotiate failed: {ex.Message}");
            }
        }
    }

    public class NegotiateRequest
    {
        public string? UserId { get; set; }
    }

    public class NegotiateResponse
    {
        public string? Url { get; set; }
        public string? AccessToken { get; set; }
    }
}
