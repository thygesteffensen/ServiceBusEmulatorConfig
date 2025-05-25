using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Encodings.Web;
using ServiceBusEmulatorConfig.Core.Models.Arm;
using ServiceBusEmulatorConfig.Core.Models.Emulator;

namespace ServiceBusEmulatorConfig.Core.Services;

public class TransformationService : ITransformationService
{
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping
    };

    private readonly JsonSerializerOptions? _deserializeOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        AllowTrailingCommas = true,
        ReadCommentHandling = JsonCommentHandling.Skip
    };

    public EmulatorConfig TransformArmToEmulatorConfig(string armJson, string namespaceName)
    {
        // Parse the ARM template
        ArmTemplate? armTemplate;
        try
        {
            armTemplate = JsonSerializer.Deserialize<ArmTemplate>(armJson, _deserializeOptions);

            if (armTemplate == null || armTemplate.Resources == null)
            {
                throw new ArgumentException("Deserialized template is null or has no resources");
            }
        }
        catch (JsonException ex)
        {
            throw new ArgumentException($"Failed to parse ARM template JSON: {ex.Message}", ex);
        }

        // Create the emulator config structure
        var emulatorConfig = new EmulatorConfig
        (
            UserConfig: new UserConfig
            (
                Namespaces:
                [
                    new Namespace(Name: namespaceName, [], [])
                ],
                Logging: new Logging(Type: "File")
            )
        );

        var serviceBusNamespace = emulatorConfig.UserConfig.Namespaces[0];

        // Group resources by type - case-insensitive comparison for robustness
        var topicResources = armTemplate.Resources
            .Where(r => r.Type.Equals("Microsoft.ServiceBus/namespaces/topics", StringComparison.OrdinalIgnoreCase))
            .ToList();

        var subscriptionResources = armTemplate.Resources
            .Where(r => r.Type.Equals("Microsoft.ServiceBus/namespaces/topics/subscriptions",
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        var ruleResources = armTemplate.Resources
            .Where(r => r.Type.Equals("Microsoft.ServiceBus/namespaces/topics/subscriptions/rules",
                StringComparison.OrdinalIgnoreCase))
            .ToList();

        Console.WriteLine(
            $"Found {topicResources.Count} topics, {subscriptionResources.Count} subscriptions, {ruleResources.Count} rules");

        // Process topics
        foreach (var topicResource in topicResources)
        {
            var topicName = ExtractResourceName(topicResource.Name, 1);
            // Replace any double quotes with single quotes in the topic name

            Console.WriteLine($"Processing topic: {topicName} (original name: {topicResource.Name})");

            var topic = new Topic
            (
                Name: topicName,
                Properties: MapTopicProperties(topicResource.Properties),
                Subscriptions: []
            );

            serviceBusNamespace.Topics.Add(topic);
        }

        // Process subscriptions
        foreach (var subscriptionResource in subscriptionResources)
        {
            // Extract topic name and subscription name
            var name = ArmHelpers.ExtractNameFromArmExpression(subscriptionResource.Name);
            var parts = name.Split('/');
            if (parts.Length < 3)
            {
                continue;
            }

            var topicName = parts[1];
            var subscriptionName = parts[2];

            // Clean up subscription name by removing trailing \u0027)]
            if (subscriptionName.EndsWith("\u0027)]"))
            {
                subscriptionName = subscriptionName.Substring(0, subscriptionName.Length - 4);
            }

            Console.WriteLine(
                $"Processing subscription: {subscriptionName} for topic: {topicName} (original name: {subscriptionResource.Name})");

            var topic = serviceBusNamespace.Topics.FirstOrDefault(t =>
                t.Name.Equals(topicName, StringComparison.OrdinalIgnoreCase));
            if (topic == null)
            {
                // Create the topic if it doesn't exist
                Console.WriteLine($"  Creating missing topic {topicName} for subscription {subscriptionName}");
                topic = new Topic
                (
                    Name: topicName,
                    Properties: new TopicProperties
                    (
                        DefaultMessageTimeToLive: "PT5M",
                        DuplicateDetectionHistoryTimeWindow: "PT5M",
                        RequiresDuplicateDetection: false,
                        DeadLetteringOnMessageExpiration: "PT5M"
                    ),
                    Subscriptions: []
                );
                serviceBusNamespace.Topics.Add(topic);
            }

            var subscription = new Subscription
            (
                Name: subscriptionName,
                Properties: MapSubscriptionProperties(subscriptionResource.Properties),
                Rules: []
            );

            topic.Subscriptions.Add(subscription);
        }

        // Process rules
        foreach (var ruleResource in ruleResources)
        {
            // Extract topic name, subscription name and rule name
            var name = ArmHelpers.ExtractNameFromArmExpression(ruleResource.Name);
            var parts = name.Split('/');
            if (parts.Length < 4)
            {
                continue;
            }

            var topicName = parts[1];
            var subscriptionName = parts[2];
            var ruleName = parts[3];

            // Clean up names by removing trailing characters
            if (subscriptionName.EndsWith("\u0027)]"))
            {
                subscriptionName = subscriptionName.Substring(0, subscriptionName.Length - 4);
            }

            var topic = serviceBusNamespace.Topics.FirstOrDefault(t =>
                t.Name.Equals(topicName, StringComparison.OrdinalIgnoreCase));
            if (topic == null)
            {
                continue;
            }

            var subscription = topic.Subscriptions.FirstOrDefault(s =>
                s.Name.Equals(subscriptionName, StringComparison.OrdinalIgnoreCase));
            if (subscription == null)
            {
                continue;
            }

            try
            {
                var rule = new Rule
                (
                    Name: ruleName,
                    Properties: MapRuleProperties(ruleResource.Properties)
                );

                subscription.Rules.Add(rule);
                Console.WriteLine($"Added rule {ruleName} to subscription {subscriptionName} in topic {topicName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing rule {ruleName}: {ex.Message}");
            }
        }

        return emulatorConfig;
    }


    public string SerializeEmulatorConfig(EmulatorConfig config)
    {
        return JsonSerializer.Serialize(config, _jsonOptions);
    }

    private string ExtractResourceName(string fullResourceName, int index)
    {
        // First process the name if it contains ARM expressions like concat
        var processedName = ArmHelpers.ExtractNameFromArmExpression(fullResourceName);

        var parts = processedName.Split('/');
        return index < parts.Length ? parts[index] : string.Empty;
    }

    private TopicProperties MapTopicProperties(ArmResourceProperties armProperties)
    {
        var properties = new TopicProperties(
            RequiresDuplicateDetection: armProperties.RequiresDuplicateDetection
        );

        return properties;
    }

    private SubscriptionProperties MapSubscriptionProperties(ArmResourceProperties armProperties)
    {
        var properties = new SubscriptionProperties(
            LockDuration: armProperties.LockDuration,
            MaxDeliveryCount: armProperties.MaxDeliveryCount,
            ForwardDeadLetteredMessagesTo: armProperties.ForwardDeadLetteredMessagesTo ?? "",
            ForwardTo: armProperties.ForwardTo ?? "",
            RequiresSession: armProperties.RequiresSession,
            DeadLetteringOnMessageExpiration: armProperties.DeadLetteringOnMessageExpiration
        );

        return properties;
    }

    private RuleProperties MapRuleProperties(ArmResourceProperties armProperties)
    {
        var properties = new RuleProperties(
            FilterType:
            armProperties.FilterType?.Equals("CorrelationFilter", StringComparison.OrdinalIgnoreCase) ?? false
                ? "Correlation"
                : "Sql",
            SqlFilter: new Models.Emulator.SqlFilter(armProperties.SqlFilter!.SqlExpression),
            Action: new SqlAction(armProperties.Action!.SqlExpression));

        return properties;
    }
}
