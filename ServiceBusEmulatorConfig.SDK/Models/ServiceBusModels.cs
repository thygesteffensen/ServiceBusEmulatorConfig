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


    public record ServiceBusTopic(
        string Name,
        TopicProperties Properties,
        List<ServiceBusSubscription> Subscriptions
    );

    public record ServiceBusSubscription(
        string Name,
        SubscriptionProperties Properties,
        List<ServiceBusRule> Rules,
        int MessageCount
    );

    public record ServiceBusRule(
        string Name,
        RuleProperties Properties
    );
}
