using Azure.Identity;
using Azure.Messaging.ServiceBus;

namespace Mvc.Services;

public class ServiceBusDemoService
{
    private readonly string _queueName;
    private readonly ServiceBusClient _serviceBusClient;
    private ServiceBusReceivedMessage _currentMessage;
    private ServiceBusReceiver _receiver;

    public ServiceBusDemoService(IConfiguration config)
    {
        var fullyQualifiedNamespace = config["ServiceBus:FullyQualifiedNamespace"];
        var tokenCredential = new DefaultAzureCredential();
        _queueName = config["ServiceBus:QueueName"];
        _serviceBusClient = new ServiceBusClient(fullyQualifiedNamespace, tokenCredential);
        _receiver = _serviceBusClient.CreateReceiver(_queueName);
    }

    public async Task SendAsync(string body)
    {
        var sender = _serviceBusClient.CreateSender(_queueName);
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
        _receiver = _serviceBusClient.CreateReceiver(_queueName);
    }
}
