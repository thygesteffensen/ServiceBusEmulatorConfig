using Azure.Messaging.ServiceBus.Administration;
using ServiceBusEmulatorConfig.Core.Models.Emulator;
using ServiceBusEmulatorConfig.Core.Models.ServiceBus;

namespace ServiceBusEmulatorConfig.Core.Clients;

public class ServiceBusExplorerClient(string connectionString)
{
    private readonly ServiceBusAdministrationClient _adminClient = new(connectionString);

    public async Task<ServiceBusNamespace> GetNamespaceDetailsAsync()
    {
        var queues = await GetQueuesAsync();
        var topics = await GetTopicsAsync();

        return new ServiceBusNamespace
        (
            ConnectionString: connectionString,
            Name: ExtractNamespaceFromConnectionString(connectionString),
            Queues: queues,
            Topics: topics
        );
    }

    public async Task<List<ServiceBusQueue>> GetQueuesAsync()
    {
        var queues = new List<ServiceBusQueue>();
        var queuesPager = _adminClient.GetQueuesAsync();

        await foreach (var queueProperties in queuesPager)
        {
            var queue = new ServiceBusQueue
            (
                Name: queueProperties.Name,
                Properties: new Models.Emulator.QueueProperties
                {
                    DeadLetteringOnMessageExpiration = queueProperties.DeadLetteringOnMessageExpiration,
                    DefaultMessageTimeToLive = queueProperties.DefaultMessageTimeToLive.ToString(),
                    DuplicateDetectionHistoryTimeWindow =
                        queueProperties.DuplicateDetectionHistoryTimeWindow.ToString(),
                    ForwardDeadLetteredMessagesTo = queueProperties.ForwardDeadLetteredMessagesTo ?? "",
                    ForwardTo = queueProperties.ForwardTo ?? "",
                    LockDuration = queueProperties.LockDuration.ToString(),
                    MaxDeliveryCount = queueProperties.MaxDeliveryCount,
                    RequiresDuplicateDetection = queueProperties.RequiresDuplicateDetection,
                    RequiresSession = queueProperties.RequiresSession
                },
                MessageCount: 0 // TODO: Get the actual message count
            );

            queues.Add(queue);
        }

        return queues;
    }

    public async Task<List<ServiceBusTopic>> GetTopicsAsync()
    {
        var topics = new List<ServiceBusTopic>();
        var topicsPager = _adminClient.GetTopicsAsync();

        await foreach (var topicProperties in topicsPager)
        {
            var subscriptions = await GetSubscriptionsAsync(topicProperties.Name);

            var topic = new ServiceBusTopic
            (
                Name: topicProperties.Name,
                Properties: new Models.Emulator.TopicProperties
                {
                    DefaultMessageTimeToLive = topicProperties.DefaultMessageTimeToLive.ToString(),
                    DuplicateDetectionHistoryTimeWindow =
                        topicProperties.DuplicateDetectionHistoryTimeWindow.ToString(),
                    RequiresDuplicateDetection = topicProperties.RequiresDuplicateDetection
                },
                Subscriptions: subscriptions
            );

            topics.Add(topic);
        }

        return topics;
    }

    public async Task<List<ServiceBusSubscription>> GetSubscriptionsAsync(string topicName)
    {
        var subscriptions = new List<ServiceBusSubscription>();
        var subscriptionsPager = _adminClient.GetSubscriptionsAsync(topicName);

        await foreach (var subscriptionProperties in subscriptionsPager)
        {
            var rules = await GetRulesAsync(topicName, subscriptionProperties.SubscriptionName);

            var subscription = new ServiceBusSubscription
            (
                Name: subscriptionProperties.SubscriptionName,
                TopicName: topicName,
                Properties: new Models.Emulator.SubscriptionProperties
                {
                    DeadLetteringOnMessageExpiration = subscriptionProperties.DeadLetteringOnMessageExpiration,
                    DefaultMessageTimeToLive = subscriptionProperties.DefaultMessageTimeToLive.ToString(),
                    LockDuration = subscriptionProperties.LockDuration.ToString(),
                    MaxDeliveryCount = subscriptionProperties.MaxDeliveryCount,
                    ForwardDeadLetteredMessagesTo = subscriptionProperties.ForwardDeadLetteredMessagesTo ?? "",
                    ForwardTo = subscriptionProperties.ForwardTo ?? "",
                    RequiresSession = subscriptionProperties.RequiresSession
                },
                Rules: rules,
                MessageCount: 0 // TODO: Get the actual message count
            );

            subscriptions.Add(subscription);
        }

        return subscriptions;
    }

    public async Task<List<ServiceBusRule>> GetRulesAsync(string topicName, string subscriptionName)
    {
        var rules = new List<ServiceBusRule>();
        var rulesPager = _adminClient.GetRulesAsync(topicName, subscriptionName);

        await foreach (var ruleProperties in rulesPager)
        {
            Models.Emulator.RuleProperties? ruleProps = null;

            if (ruleProperties.Filter is SqlRuleFilter sqlFilter)
            {
                ruleProps = new Models.Emulator.RuleProperties
                {
                    FilterType = "Sql",
                    SqlFilter = new SqlFilter
                    {
                        SqlExpression = sqlFilter.SqlExpression
                    }
                };
            }
            else if (ruleProperties.Filter is CorrelationRuleFilter correlationFilter)
            {
                var properties = new Dictionary<string, string>();
                foreach (var prop in correlationFilter.ApplicationProperties)
                {
                    properties.Add(prop.Key, prop.Value?.ToString() ?? "");
                }

                ruleProps = new Models.Emulator.RuleProperties
                {
                    FilterType = "Correlation",
                    CorrelationFilter = new CorrelationFilter
                    {
                        ContentType = correlationFilter.ContentType ?? "",
                        CorrelationId = correlationFilter.CorrelationId ?? "",
                        Label = "", // Azure SDK's CorrelationRuleFilter doesn't have Label
                        MessageId = correlationFilter.MessageId ?? "",
                        Properties = properties,
                        ReplyTo = correlationFilter.ReplyTo ?? "",
                        ReplyToSessionId = correlationFilter.ReplyToSessionId ?? "",
                        SessionId = correlationFilter.SessionId ?? "",
                        To = correlationFilter.To ?? ""
                    }
                };
            }

            if (ruleProperties.Action is SqlRuleAction sqlAction)
            {
                ruleProps ??= new Models.Emulator.RuleProperties();

                ruleProps.Action = new SqlAction
                {
                    SqlExpression = sqlAction.SqlExpression
                };
            }

            rules.Add(new ServiceBusRule
            (
                Name: ruleProperties.Name,
                Properties: ruleProps!
            ));
        }

        return rules;
    }

    public async Task<EmulatorConfig> ExportToEmulatorConfigAsync()
    {
        var serviceBusNamespace = await GetNamespaceDetailsAsync();

        // Create the emulator config structure
        var emulatorConfig = new EmulatorConfig
        {
            UserConfig = new UserConfig
            {
                Namespaces =
                [
                    new Namespace
                    {
                        Name = serviceBusNamespace.Name,
                        Queues =
                        [
                            .. serviceBusNamespace.Queues.Select(q => new Queue
                            {
                                Name = q.Name,
                                Properties = q.Properties
                            })
                        ],
                        Topics =
                        [
                            .. serviceBusNamespace.Topics.Select(t => new Topic
                            {
                                Name = t.Name,
                                Properties = t.Properties,
                                Subscriptions =
                                [
                                    .. t.Subscriptions.Select(s => new Subscription
                                    {
                                        Name = s.Name,
                                        Properties = s.Properties,
                                        Rules =
                                        [
                                            .. s.Rules.Select(r => new Rule
                                            {
                                                Name = r.Name,
                                                Properties = r.Properties
                                            })
                                        ]
                                    })
                                ]
                            })
                        ]
                    }
                ],
                Logging = new Logging { Type = "File" }
            }
        };

        return emulatorConfig;
    }

    private string ExtractNamespaceFromConnectionString(string connString)
    {
        var parts = connString.Split(';');
        foreach (var part in parts)
        {
            if (!part.StartsWith("Endpoint=", StringComparison.OrdinalIgnoreCase)) continue;

            var endpoint = part.Substring("Endpoint=".Length);
            var uri = new Uri(endpoint);
            return uri.Host.Split('.')[0];
        }

        return "defaultnamespace";
    }
}
