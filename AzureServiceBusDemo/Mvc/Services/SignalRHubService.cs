using Microsoft.AspNetCore.SignalR;

namespace Mvc.Services
{
    public class SignalRHubService : Hub
    {
        public async Task BroadcastFromFunction(string message)
        {
            // send message to all connected clients
            // in this case the javscript function "ReceiveMessageFromAzureFunction" will be called on the client side inside: AzureFunctionHttpTrigger\Index.cshtml
            await Clients.All.SendAsync("ReceiveMessageFromAzureFunction", message); 
        }
    }
}
