using System.Collections.ObjectModel;
using ServiceBusEmulatorConfig.Core.Models.Emulator;

namespace ServiceBusEmulatorConfig.SDK.Models
{
    public class ServiceBusNamespace
    {
        public string Name { get; set; } = string.Empty;
        public string ConnectionString { get; set; } = string.Empty;
        public List<ServiceBusQueue> Queues { get; set; } = new List<ServiceBusQueue>();
        public List<ServiceBusTopic> Topics { get; set; } = new List<ServiceBusTopic>();
    }

    public class ServiceBusQueue
    {
        public string Name { get; set; } = string.Empty;
        public QueueProperties Properties { get; set; } = new QueueProperties();
        public int MessageCount { get; set; }
    }

    public class ServiceBusTopic
    {
        public string Name { get; set; } = string.Empty;
        public TopicProperties Properties { get; set; } = new TopicProperties();
        public List<ServiceBusSubscription> Subscriptions { get; set; } = new List<ServiceBusSubscription>();
    }

    public class ServiceBusSubscription
    {
        public string Name { get; set; } = string.Empty;
        public SubscriptionProperties Properties { get; set; } = new SubscriptionProperties();
        public List<ServiceBusRule> Rules { get; set; } = new List<ServiceBusRule>();
        public int MessageCount { get; set; }
    }

    public class ServiceBusRule
    {
        public string Name { get; set; } = string.Empty;
        public RuleProperties Properties { get; set; } = new RuleProperties();
    }
}
