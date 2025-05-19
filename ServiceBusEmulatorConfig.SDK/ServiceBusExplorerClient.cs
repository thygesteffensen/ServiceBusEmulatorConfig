using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using System.Collections.ObjectModel;
using ServiceBusEmulatorConfig.Core.Models.Emulator;
using ServiceBusEmulatorConfig.SDK.Models;

namespace ServiceBusEmulatorConfig.SDK
{
    public class ServiceBusExplorerClient
    {
        private readonly string _connectionString;
        private readonly ServiceBusAdministrationClient _adminClient;
        private readonly ServiceBusClient _serviceBusClient;

        public ServiceBusExplorerClient(string connectionString)
        {
            _connectionString = connectionString;
            _adminClient = new ServiceBusAdministrationClient(connectionString);
            _serviceBusClient = new ServiceBusClient(connectionString);
        }

        public async Task<ServiceBusNamespace> GetNamespaceDetailsAsync()
        {
            var queues = await GetQueuesAsync();
            var topics = await GetTopicsAsync();

            return new ServiceBusNamespace
            {
                ConnectionString = _connectionString,
                Name = ExtractNamespaceFromConnectionString(_connectionString),
                Queues = queues,
                Topics = topics
            };
        }

        public async Task<List<ServiceBusQueue>> GetQueuesAsync()
        {
            var queues = new List<ServiceBusQueue>();
            var queuesPager = _adminClient.GetQueuesAsync();

            await foreach (var queueProperties in queuesPager)
            {
                var queue = new ServiceBusQueue
                {
                    Name = queueProperties.Name,
                    Properties = new Core.Models.Emulator.QueueProperties
                    {
                        DeadLetteringOnMessageExpiration = queueProperties.DeadLetteringOnMessageExpiration,
                        DefaultMessageTimeToLive = queueProperties.DefaultMessageTimeToLive.ToString(),
                        DuplicateDetectionHistoryTimeWindow = queueProperties.DuplicateDetectionHistoryTimeWindow.ToString(),
                        ForwardDeadLetteredMessagesTo = queueProperties.ForwardDeadLetteredMessagesTo ?? "",
                        ForwardTo = queueProperties.ForwardTo ?? "",
                        LockDuration = queueProperties.LockDuration.ToString(),
                        MaxDeliveryCount = queueProperties.MaxDeliveryCount,
                        RequiresDuplicateDetection = queueProperties.RequiresDuplicateDetection,
                        RequiresSession = queueProperties.RequiresSession
                    }
                };

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
                {
                    Name = topicProperties.Name,
                    Properties = new Core.Models.Emulator.TopicProperties
                    {
                        DefaultMessageTimeToLive = topicProperties.DefaultMessageTimeToLive.ToString(),
                        DuplicateDetectionHistoryTimeWindow = topicProperties.DuplicateDetectionHistoryTimeWindow.ToString(),
                        RequiresDuplicateDetection = topicProperties.RequiresDuplicateDetection
                    },
                    Subscriptions = subscriptions
                };

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
                {
                    Name = subscriptionProperties.SubscriptionName,
                    Properties = new Core.Models.Emulator.SubscriptionProperties
                    {
                        DeadLetteringOnMessageExpiration = subscriptionProperties.DeadLetteringOnMessageExpiration,
                        DefaultMessageTimeToLive = subscriptionProperties.DefaultMessageTimeToLive.ToString(),
                        LockDuration = subscriptionProperties.LockDuration.ToString(),
                        MaxDeliveryCount = subscriptionProperties.MaxDeliveryCount,
                        ForwardDeadLetteredMessagesTo = subscriptionProperties.ForwardDeadLetteredMessagesTo ?? "",
                        ForwardTo = subscriptionProperties.ForwardTo ?? "",
                        RequiresSession = subscriptionProperties.RequiresSession
                    },
                    Rules = rules
                };

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
                var rule = new ServiceBusRule
                {
                    Name = ruleProperties.Name
                };

                if (ruleProperties.Filter is SqlRuleFilter sqlFilter)
                {
                    rule.Properties = new Core.Models.Emulator.RuleProperties
                    {
                        FilterType = "Sql",
                        SqlFilter = new Core.Models.Emulator.SqlFilter
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

                    rule.Properties = new Core.Models.Emulator.RuleProperties
                    {
                        FilterType = "Correlation",
                        CorrelationFilter = new Core.Models.Emulator.CorrelationFilter
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
                    if (rule.Properties == null)
                    {
                        rule.Properties = new Core.Models.Emulator.RuleProperties();
                    }

                    rule.Properties.Action = new Core.Models.Emulator.SqlAction
                    {
                        SqlExpression = sqlAction.SqlExpression
                    };
                }

                rules.Add(rule);
            }

            return rules;
        }

        public async Task<EmulatorConfig> ExportToEmulatorConfigAsync()
        {
            var namespace_ = await GetNamespaceDetailsAsync();
            
            // Create the emulator config structure
            var emulatorConfig = new EmulatorConfig
            {
                UserConfig = new UserConfig
                {
                    Namespaces = 
                    [
                        new Namespace
                        {
                            Name = namespace_.Name,
                            Queues = namespace_.Queues.Select(q => new Queue
                            {
                                Name = q.Name,
                                Properties = q.Properties
                            }).ToList(),
                            Topics = namespace_.Topics.Select(t => new Topic
                            {
                                Name = t.Name,
                                Properties = t.Properties,
                                Subscriptions = t.Subscriptions.Select(s => new Subscription
                                {
                                    Name = s.Name,
                                    Properties = s.Properties,
                                    Rules = s.Rules.Select(r => new Rule
                                    {
                                        Name = r.Name,
                                        Properties = r.Properties
                                    }).ToList()
                                }).ToList()
                            }).ToList()
                        }
                    ],
                    Logging = new Logging { Type = "File" }
                }
            };

            return emulatorConfig;
        }

        private string ExtractNamespaceFromConnectionString(string connectionString)
        {
            var parts = connectionString.Split(';');
            foreach (var part in parts)
            {
                if (part.StartsWith("Endpoint=", StringComparison.OrdinalIgnoreCase))
                {
                    var endpoint = part.Substring("Endpoint=".Length);
                    var uri = new Uri(endpoint);
                    return uri.Host.Split('.')[0];
                }
            }

            return "defaultnamespace";
        }
    }
}
