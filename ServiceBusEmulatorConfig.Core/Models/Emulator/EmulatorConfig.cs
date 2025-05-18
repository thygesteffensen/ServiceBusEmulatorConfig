using System.Text.Json.Serialization;

namespace ServiceBusEmulatorConfig.Core.Models.Emulator;

public class EmulatorConfig
{
    [JsonPropertyName("UserConfig")]
    public UserConfig UserConfig { get; set; }
}

public class UserConfig
{
    [JsonPropertyName("Namespaces")]
    public List<Namespace> Namespaces { get; set; }

    [JsonPropertyName("Logging")]
    public Logging Logging { get; set; }
}

public class Logging
{
    [JsonPropertyName("Type")]
    public string Type { get; set; } = "File";
}

public class Namespace
{
    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("Queues")]
    public List<Queue> Queues { get; set; } = new List<Queue>();

    [JsonPropertyName("Topics")]
    public List<Topic> Topics { get; set; } = new List<Topic>();
}

public class Queue
{
    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("Properties")]
    public QueueProperties Properties { get; set; }
}

public class QueueProperties
{
    [JsonPropertyName("DeadLetteringOnMessageExpiration")]
    public bool DeadLetteringOnMessageExpiration { get; set; }

    [JsonPropertyName("DefaultMessageTimeToLive")]
    public string DefaultMessageTimeToLive { get; set; }

    [JsonPropertyName("DuplicateDetectionHistoryTimeWindow")]
    public string DuplicateDetectionHistoryTimeWindow { get; set; }

    [JsonPropertyName("ForwardDeadLetteredMessagesTo")]
    public string ForwardDeadLetteredMessagesTo { get; set; }

    [JsonPropertyName("ForwardTo")]
    public string ForwardTo { get; set; }

    [JsonPropertyName("LockDuration")]
    public string LockDuration { get; set; }

    [JsonPropertyName("MaxDeliveryCount")]
    public int MaxDeliveryCount { get; set; }

    [JsonPropertyName("RequiresDuplicateDetection")]
    public bool RequiresDuplicateDetection { get; set; }

    [JsonPropertyName("RequiresSession")]
    public bool RequiresSession { get; set; }
}

public class Topic
{
    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("Properties")]
    public TopicProperties Properties { get; set; }

    [JsonPropertyName("Subscriptions")]
    public List<Subscription> Subscriptions { get; set; } = new List<Subscription>();
}

public class TopicProperties
{
    [JsonPropertyName("DefaultMessageTimeToLive")]
    public string DefaultMessageTimeToLive { get; set; }

    [JsonPropertyName("DuplicateDetectionHistoryTimeWindow")]
    public string DuplicateDetectionHistoryTimeWindow { get; set; }

    [JsonPropertyName("RequiresDuplicateDetection")]
    public bool RequiresDuplicateDetection { get; set; }
}

public class Subscription
{
    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("Properties")]
    public SubscriptionProperties Properties { get; set; }

    [JsonPropertyName("Rules")]
    public List<Rule> Rules { get; set; } = new List<Rule>();
}

public class SubscriptionProperties
{
    [JsonPropertyName("DeadLetteringOnMessageExpiration")]
    public bool DeadLetteringOnMessageExpiration { get; set; }

    [JsonPropertyName("DefaultMessageTimeToLive")]
    public string DefaultMessageTimeToLive { get; set; }

    [JsonPropertyName("LockDuration")]
    public string LockDuration { get; set; }

    [JsonPropertyName("MaxDeliveryCount")]
    public int MaxDeliveryCount { get; set; }

    [JsonPropertyName("ForwardDeadLetteredMessagesTo")]
    public string ForwardDeadLetteredMessagesTo { get; set; }

    [JsonPropertyName("ForwardTo")]
    public string ForwardTo { get; set; }

    [JsonPropertyName("RequiresSession")]
    public bool RequiresSession { get; set; }
}

public class Rule
{
    [JsonPropertyName("Name")]
    public string Name { get; set; }

    [JsonPropertyName("Properties")]
    public RuleProperties Properties { get; set; }
}

public class RuleProperties
{
    [JsonPropertyName("FilterType")]
    public string FilterType { get; set; }

    [JsonPropertyName("SqlFilter")]
    public SqlFilter SqlFilter { get; set; }

    [JsonPropertyName("CorrelationFilter")]
    public CorrelationFilter CorrelationFilter { get; set; }

    [JsonPropertyName("Action")]
    public SqlAction Action { get; set; }
}

public class SqlFilter
{
    [JsonPropertyName("SqlExpression")]
    public string SqlExpression { get; set; }
}

public class SqlAction
{
    [JsonPropertyName("SqlExpression")]
    public string SqlExpression { get; set; }
}

public class CorrelationFilter
{
    [JsonPropertyName("ContentType")]
    public string ContentType { get; set; }

    [JsonPropertyName("CorrelationId")]
    public string CorrelationId { get; set; }

    [JsonPropertyName("Label")]
    public string Label { get; set; }

    [JsonPropertyName("MessageId")]
    public string MessageId { get; set; }

    [JsonPropertyName("Properties")]
    public Dictionary<string, string> Properties { get; set; }

    [JsonPropertyName("ReplyTo")]
    public string ReplyTo { get; set; }

    [JsonPropertyName("ReplyToSessionId")]
    public string ReplyToSessionId { get; set; }

    [JsonPropertyName("SessionId")]
    public string SessionId { get; set; }

    [JsonPropertyName("To")]
    public string To { get; set; }
}