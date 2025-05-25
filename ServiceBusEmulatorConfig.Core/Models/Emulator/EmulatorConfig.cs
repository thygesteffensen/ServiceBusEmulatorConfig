namespace ServiceBusEmulatorConfig.Core.Models.Emulator;

public record EmulatorConfig(UserConfig UserConfig);

public record UserConfig(List<Namespace> Namespaces, Logging Logging);

public record Logging(string Type);

public record Namespace(string Name, List<Queue> Queues, List<Topic> Topics);

public record Queue(string Name, QueueProperties Properties);

public record QueueProperties(
    bool DeadLetteringOnMessageExpiration,
    string DefaultMessageTimeToLive,
    string DuplicateDetectionHistoryTimeWindow,
    string ForwardDeadLetteredMessagesTo,
    string ForwardTo,
    string LockDuration,
    int MaxDeliveryCount,
    bool RequiresDuplicateDetection,
    bool RequiresSession
);

public record Topic(string Name, TopicProperties Properties, List<Subscription> Subscriptions);

public record TopicProperties(
    bool RequiresDuplicateDetection,
    string DeadLetteringOnMessageExpiration = "PT5M",
    string DefaultMessageTimeToLive = "PT5M",
    string DuplicateDetectionHistoryTimeWindow = "PT5M"
);

public record Subscription(string Name, SubscriptionProperties Properties, List<Rule> Rules);

public record SubscriptionProperties(
    bool DeadLetteringOnMessageExpiration,
    string LockDuration,
    int MaxDeliveryCount,
    string ForwardDeadLetteredMessagesTo,
    string ForwardTo,
    bool RequiresSession,
    string DefaultMessageTimeToLive = "PT5M"
);

public record Rule(string Name, RuleProperties Properties);

public record RuleProperties(
    string FilterType,
    SqlFilter SqlFilter,
    SqlAction Action,
    CorrelationFilter? CorrelationFilter = null
);

public record SqlFilter(string SqlExpression);

public record SqlAction(string SqlExpression);

public record CorrelationFilter(
    string ContentType,
    string Label,
    string MessageId,
    string ReplyTo,
    string ReplyToSessionId,
    string SessionId,
    string To
);
