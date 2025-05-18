using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using ServiceBusEmulatorConfig.Core.Models.Arm;
using ServiceBusEmulatorConfig.Core.Models.Emulator;

namespace ServiceBusEmulatorConfig.Core.Services
{
    public class TransformationService : ITransformationService
    {
        private readonly JsonSerializerOptions _jsonOptions;
        private readonly JsonSerializerOptions _deserializeOptions;

        public TransformationService()
        {
            _jsonOptions = new JsonSerializerOptions
            {
                WriteIndented = true,
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
            };

            _deserializeOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                AllowTrailingCommas = true,
                ReadCommentHandling = JsonCommentHandling.Skip
            };
        }

        public EmulatorConfig TransformArmToEmulatorConfig(string armJson, string namespaceName)
        {
            // Parse the ARM template
            ArmTemplate armTemplate;
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
            {
                UserConfig = new UserConfig
                {
                    Namespaces = new List<Namespace>
                    {
                        new Namespace
                        {
                            Name = namespaceName
                        }
                    },
                    Logging = new Logging { Type = "File" }
                }
            };

            var namespace_ = emulatorConfig.UserConfig.Namespaces[0];

            // Group resources by type - case insensitive comparison for robustness
            var topicResources = armTemplate.Resources
                .Where(r => r.Type.Equals("Microsoft.ServiceBus/namespaces/topics", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var subscriptionResources = armTemplate.Resources
                .Where(r => r.Type.Equals("Microsoft.ServiceBus/namespaces/topics/subscriptions", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var ruleResources = armTemplate.Resources
                .Where(r => r.Type.Equals("Microsoft.ServiceBus/namespaces/topics/subscriptions/rules", StringComparison.OrdinalIgnoreCase))
                .ToList();

            Console.WriteLine($"Found {topicResources.Count} topics, {subscriptionResources.Count} subscriptions, {ruleResources.Count} rules");

            // Process topics
            foreach (var topicResource in topicResources)
            {
                var topicName = ExtractResourceName(topicResource.Name, 1);
                Console.WriteLine($"Processing topic: {topicName} (original name: {topicResource.Name})");
                
                var topic = new Topic
                {
                    Name = topicName,
                    Properties = MapTopicProperties(topicResource.Properties),
                    Subscriptions = new List<Subscription>()
                };

                namespace_.Topics.Add(topic);
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

                Console.WriteLine($"Processing subscription: {subscriptionName} for topic: {topicName} (original name: {subscriptionResource.Name})");
                
                var topic = namespace_.Topics.FirstOrDefault(t => t.Name.Equals(topicName, StringComparison.OrdinalIgnoreCase));
                if (topic == null)
                {
                    // Create the topic if it doesn't exist
                    Console.WriteLine($"  Creating missing topic {topicName} for subscription {subscriptionName}");
                    topic = new Topic
                    {
                        Name = topicName,
                        Properties = new TopicProperties
                        {
                            DefaultMessageTimeToLive = "P10675199DT2H48M5.4775807S",
                            DuplicateDetectionHistoryTimeWindow = "PT10M",
                            RequiresDuplicateDetection = false
                        },
                        Subscriptions = new List<Subscription>()
                    };
                    namespace_.Topics.Add(topic);
                }

                var subscription = new Subscription
                {
                    Name = subscriptionName,
                    Properties = MapSubscriptionProperties(subscriptionResource.Properties),
                    Rules = new List<Rule>()
                };

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

                var topic = namespace_.Topics.FirstOrDefault(t => t.Name.Equals(topicName, StringComparison.OrdinalIgnoreCase));
                if (topic == null)
                {
                    continue;
                }

                var subscription = topic.Subscriptions.FirstOrDefault(s => s.Name.Equals(subscriptionName, StringComparison.OrdinalIgnoreCase));
                if (subscription == null)
                {
                    continue;
                }

                try
                {
                    var rule = new Rule
                    {
                        Name = ruleName,
                        Properties = MapRuleProperties(ruleResource.Properties)
                    };

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

        public async Task<EmulatorConfig> TransformArmToEmulatorConfigAsync(string armJsonPath, string namespaceName)
        {
            try
            {
                var armJson = await File.ReadAllTextAsync(armJsonPath);
                return TransformArmToEmulatorConfig(armJson, namespaceName);
            }
            catch (Exception ex) when (ex is not ArgumentException)
            {
                throw new ArgumentException($"Failed to read or process file {armJsonPath}: {ex.Message}", ex);
            }
        }

        public string SerializeEmulatorConfig(EmulatorConfig config)
        {
            return JsonSerializer.Serialize(config, _jsonOptions);
        }

        private string ExtractResourceName(string fullResourceName, int index)
        {
            // First process the name if it contains ARM expressions like concat
            string processedName = ArmHelpers.ExtractNameFromArmExpression(fullResourceName);
            
            var parts = processedName.Split('/');
            return index < parts.Length ? parts[index] : string.Empty;
        }

        private TopicProperties MapTopicProperties(Dictionary<string, object> armProperties)
        {
            var properties = new TopicProperties();

            if (armProperties == null)
            {
                return properties;
            }

            if (armProperties.TryGetValue("defaultMessageTimeToLive", out var defaultTtl))
            {
                properties.DefaultMessageTimeToLive = defaultTtl?.ToString() ?? "PT1H";
            }
            else
            {
                properties.DefaultMessageTimeToLive = "PT1H"; // Default 1 hour
            }

            if (armProperties.TryGetValue("duplicateDetectionHistoryTimeWindow", out var ddWindow))
            {
                properties.DuplicateDetectionHistoryTimeWindow = ddWindow?.ToString() ?? "PT10M";
            }
            else
            {
                properties.DuplicateDetectionHistoryTimeWindow = "PT10M"; // Default 10 minutes
            }

            if (armProperties.TryGetValue("requiresDuplicateDetection", out var reqDupDetection) && reqDupDetection != null)
            {
                if (reqDupDetection is JsonElement jsonElement)
                {
                    properties.RequiresDuplicateDetection = jsonElement.ValueKind == JsonValueKind.True;
                }
                else
                {
                    properties.RequiresDuplicateDetection = Convert.ToBoolean(reqDupDetection);
                }
            }

            return properties;
        }

        private SubscriptionProperties MapSubscriptionProperties(Dictionary<string, object> armProperties)
        {
            var properties = new SubscriptionProperties();

            if (armProperties == null)
            {
                return properties;
            }

            if (armProperties.TryGetValue("deadLetteringOnMessageExpiration", out var deadLettering) && deadLettering != null)
            {
                if (deadLettering is JsonElement jsonElement)
                {
                    properties.DeadLetteringOnMessageExpiration = jsonElement.ValueKind == JsonValueKind.True;
                }
                else
                {
                    properties.DeadLetteringOnMessageExpiration = Convert.ToBoolean(deadLettering);
                }
            }

            if (armProperties.TryGetValue("defaultMessageTimeToLive", out var defaultTtl))
            {
                properties.DefaultMessageTimeToLive = defaultTtl?.ToString() ?? "PT1H";
            }
            else
            {
                properties.DefaultMessageTimeToLive = "PT1H"; // Default 1 hour
            }

            if (armProperties.TryGetValue("lockDuration", out var lockDuration))
            {
                properties.LockDuration = lockDuration?.ToString() ?? "PT1M";
            }
            else
            {
                properties.LockDuration = "PT1M"; // Default 1 minute
            }

            if (armProperties.TryGetValue("maxDeliveryCount", out var maxDelivery) && maxDelivery != null)
            {
                if (maxDelivery is JsonElement jsonElement)
                {
                    if (jsonElement.ValueKind == JsonValueKind.Number)
                    {
                        properties.MaxDeliveryCount = jsonElement.GetInt32();
                    }
                }
                else
                {
                    properties.MaxDeliveryCount = Convert.ToInt32(maxDelivery);
                }
            }
            else
            {
                properties.MaxDeliveryCount = 10; // Default
            }

            if (armProperties.TryGetValue("forwardDeadLetteredMessagesTo", out var forwardDlq))
            {
                properties.ForwardDeadLetteredMessagesTo = forwardDlq?.ToString() ?? "";
            }
            else
            {
                properties.ForwardDeadLetteredMessagesTo = "";
            }

            if (armProperties.TryGetValue("forwardTo", out var forwardTo))
            {
                properties.ForwardTo = forwardTo?.ToString() ?? "";
            }
            else
            {
                properties.ForwardTo = "";
            }

            if (armProperties.TryGetValue("requiresSession", out var reqSession) && reqSession != null)
            {
                if (reqSession is JsonElement jsonElement)
                {
                    properties.RequiresSession = jsonElement.ValueKind == JsonValueKind.True;
                }
                else
                {
                    properties.RequiresSession = Convert.ToBoolean(reqSession);
                }
            }

            return properties;
        }

        private RuleProperties MapRuleProperties(Dictionary<string, object> armProperties)
        {
            var properties = new RuleProperties();

            if (armProperties == null)
            {
                return properties;
            }

            // Default to Sql Filter type
            properties.FilterType = "Sql";

            if (armProperties.TryGetValue("filterType", out var filterType) && filterType != null)
            {
                if (filterType.ToString().Equals("CorrelationFilter", StringComparison.OrdinalIgnoreCase))
                {
                    properties.FilterType = "Correlation";
                    // Handle correlation filter mapping if needed
                }
            }

            // Handle SQL filter
            try
            {
                if (armProperties.TryGetValue("sqlFilter", out var sqlFilterObj))
                {
                    properties.SqlFilter = new SqlFilter();
                    
                    if (sqlFilterObj is JsonElement sqlFilter && sqlFilter.TryGetProperty("sqlExpression", out var sqlExpr))
                    {
                        properties.SqlFilter.SqlExpression = sqlExpr.GetString() ?? "";
                    }
                    else if (sqlFilterObj is Dictionary<string, object> sqlFilterDict && 
                             sqlFilterDict.TryGetValue("sqlExpression", out var sqlExprObj))
                    {
                        properties.SqlFilter.SqlExpression = sqlExprObj?.ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing SQL filter: {ex.Message}");
                properties.SqlFilter = new SqlFilter { SqlExpression = "" };
            }

            // Handle action
            try 
            {
                if (armProperties.TryGetValue("action", out var actionObj))
                {
                    properties.Action = new SqlAction();
                    
                    if (actionObj is JsonElement action && action.TryGetProperty("sqlExpression", out var actionExpr))
                    {
                        properties.Action.SqlExpression = actionExpr.GetString() ?? "";
                    }
                    else if (actionObj is Dictionary<string, object> actionDict && 
                             actionDict.TryGetValue("sqlExpression", out var actionExprObj))
                    {
                        properties.Action.SqlExpression = actionExprObj?.ToString() ?? "";
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing action: {ex.Message}");
                properties.Action = new SqlAction { SqlExpression = "" };
            }

            return properties;
        }
    }
}
