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

public class AzureServiceBusTopicService
{
    private readonly string _topicName;
    private readonly ServiceBusClient _serviceBusClient;
    private ServiceBusReceivedMessage _currentMessage;
    private ServiceBusReceiver _receiver;

    public AzureServiceBusTopicService(IConfiguration config)
    {
        var fullyQualifiedNamespace = config["ServiceBus:FullyQualifiedNamespace"];
        var tokenCredential = new DefaultAzureCredential();
        _topicName = config["ServiceBus:TopicName"];
        _serviceBusClient = new ServiceBusClient(fullyQualifiedNamespace, tokenCredential);
        _receiver = _serviceBusClient.CreateReceiver(_topicName);
    }

    public async Task SendAsync(string body)
    {
        var sender = _serviceBusClient.CreateSender(_topicName);
        await sender.SendMessageAsync(new ServiceBusMessage(body));
    }

    public async Task<IReadOnlyList<ServiceBusReceivedMessage>> PeekAsync()
    {
        var maxPeekListCount = 10;
        var peekListStartPosition = 0; // if we added a new message the peek will start from that and we'll only get that one.
        return await _receiver.PeekMessagesAsync(maxPeekListCount, peekListStartPosition);
    }

    public async Task<ServiceBusReceivedMessage?> ReceiveAsync()
    {
        var _maxWait = TimeSpan.FromSeconds(5);

        _currentMessage = await _receiver.ReceiveMessageAsync(_maxWait);
        return _currentMessage;
    }

    public ServiceBusReceivedMessage GetCurrentMessage()
    {
        return _currentMessage;
    }

    public async Task CompleteAsync(ServiceBusReceivedMessage msg)
    {
        if (_currentMessage != null && _currentMessage.LockToken == msg.LockToken)
            await _receiver.CompleteMessageAsync(msg);

        _currentMessage = null;
        ResetReceiver();
    }

    public async Task AbandonAsync(ServiceBusReceivedMessage msg)
    {
        if (_currentMessage != null && _currentMessage.LockToken == msg.LockToken)
            await _receiver.AbandonMessageAsync(msg);

        _currentMessage = null;
        ResetReceiver();
    }

    public async Task DeadLetterAsync(ServiceBusReceivedMessage msg)
    {
        if (_currentMessage != null && _currentMessage.LockToken == msg.LockToken)
            await _receiver.DeadLetterMessageAsync(msg, "Demo dead-letter");

        _currentMessage = null;
        ResetReceiver();
    }

    private void ResetReceiver()
    {
        _receiver = _serviceBusClient.CreateReceiver(_topicName);
    }
}
