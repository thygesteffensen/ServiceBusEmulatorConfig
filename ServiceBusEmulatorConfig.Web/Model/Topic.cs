using Azure.Messaging.ServiceBus.Administration;

namespace ServiceBusEmulatorConfig.Web.Model;

public record Topic(
    string Name,
    TopicProperties Properties,
    TopicRuntimeProperties RuntimeProperties,
    List<Subscription> Subscriptions
)
{
    public Identifier Identifier { get; } = new TopicIdentifier(Name);
}

public record Subscription(
    string Name,
    string TopicName,
    SubscriptionProperties Properties,
    SubscriptionRuntimeProperties RuntimeProperties,
    List<RuleProperties> Rules
)
{
    public Identifier Identifier { get; } = new SubscriptionIdentifier(TopicName, Name);
}

public record Identifier;

public record TopicIdentifier(string TopicName) : Identifier
{
    public override string ToString() => TopicName;
}

public record SubscriptionIdentifier(string TopicName, string SubscriptionName) : Identifier
{
    public override string ToString() => $"{TopicName}/{SubscriptionName}";
}
