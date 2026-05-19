namespace Mvc.Models;

public class TopicViewModel
{
    public IReadOnlyList<Azure.Messaging.ServiceBus.ServiceBusReceivedMessage> Peeked { get; set; }
    public Azure.Messaging.ServiceBus.ServiceBusReceivedMessage Received { get; set; }
}

