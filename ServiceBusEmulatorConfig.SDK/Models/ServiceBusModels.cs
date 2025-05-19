using ServiceBusEmulatorConfig.Core.Models.Emulator;

namespace ServiceBusEmulatorConfig.SDK.Models
{
    public record ServiceBusNamespace(
        string Name,
        string ConnectionString,
        List<ServiceBusQueue> Queues,
        List<ServiceBusTopic> Topics
    );

    public record ServiceBusQueue(
        string Name,
        QueueProperties Properties,
        int MessageCount
    );


    public record Identifier;
    public record TopicIdentifier(string TopicName) : Identifier
    {
        public override string ToString()
        {
            return TopicName;
        }
    };
    public record SubscriptionIdentifier(string TopicName, string SubscriptionName) : Identifier
    {
        public override string ToString()
        {
            return $"{TopicName}/{SubscriptionName}";
        }
    };

    public record ServiceBusTopic(
        string Name,
        TopicProperties Properties,
        List<ServiceBusSubscription> Subscriptions
    )
    {
        private readonly Identifier _identifier = new TopicIdentifier(Name);
        public Identifier Identifier => _identifier;
    };

    public record ServiceBusSubscription(
        string Name,
        string TopicName,
        SubscriptionProperties Properties,
        List<ServiceBusRule> Rules,
        int MessageCount
    )
    {
        private readonly Identifier _identifier = new SubscriptionIdentifier(TopicName, Name);
        public Identifier Identifier => _identifier;
    };

    public record ServiceBusRule(
        string Name,
        RuleProperties Properties
    );
}
