using Azure.Identity;
using Azure.Messaging.ServiceBus;

namespace Mvc.Services;

/*
    SERVICE BUS AUTHENTICATION (AZURE SETUP REQUIRED)
    Uses DefaultAzureCredential() to auth Azure Service Bus.
    Relies on Managed Identity of the Azure App Service at runtime.
    Azure configuration:
    1. Enable Managed Identity on the App Service (AzureServiceBusDemoMvc):
       - Azure Portal → App Service → Settings → Identity → System assigned → On

    2. Grant the App Service access to the Service Bus namespace (wizzard262):
       - Azure Portal → Service Bus Namespace → Access Control (IAM)
       - Add role assignments:
            • Azure Service Bus Data Sender
            • Azure Service Bus Data Receiver
       - Assign these roles to the App Service’s Managed Identity

    3. Ensure required configuration values exist in App Service settings:
       - ServiceBus:FullyQualifiedNamespace = "<yournamespace>.servicebus.windows.net"
       - ServiceBus:QueueName = "<your-queue-name>"

    Without these Azure permissions, DefaultAzureCredential() will fail in the cloud
    (even though it works locally using your developer identity), causing 500 errors.
*/

public class SignalRAzureServiceBusQueueService
{
    private readonly string _queueName;
    private readonly ServiceBusClient _serviceBusClient;

    public SignalRAzureServiceBusQueueService(IConfiguration config)
    {
        var fullyQualifiedNamespace = config["ServiceBus:FullyQualifiedNamespace"];
        var tokenCredential = new DefaultAzureCredential();
        _queueName = config["ServiceBus:SignalRQueueName"];
        _serviceBusClient = new ServiceBusClient(fullyQualifiedNamespace, tokenCredential);
    }

    public async Task SendAsync(string body)
    {
        var sender = _serviceBusClient.CreateSender(_queueName);
        await sender.SendMessageAsync(new ServiceBusMessage(body));
    }
}
