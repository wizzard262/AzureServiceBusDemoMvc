namespace Mvc.Models;

public class QueueViewModel
{
    public IReadOnlyList<Azure.Messaging.ServiceBus.ServiceBusReceivedMessage> Peeked { get; set; }
    public Azure.Messaging.ServiceBus.ServiceBusReceivedMessage Received { get; set; }
}

