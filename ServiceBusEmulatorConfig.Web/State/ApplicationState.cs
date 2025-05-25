using Azure.Messaging.ServiceBus.Administration;
using ServiceBusEmulatorConfig.Web.Model;
using ServiceBusEmulatorConfig.Web.Pages;

namespace ServiceBusEmulatorConfig.Web.State;

public interface IApplicationState
{
    bool IsConnected { get; }
    string ConnectionString { get; }
    Task Connect(string connectionString);
    void Disconnect();

    NamespaceProperties NamespaceProperties { get; }
    Queues.PageState QueuePageState { get; }
    Topics.PageState TopicPageState { get; }
}

public class ApplicationState : IApplicationState
{
    private ServiceBusAdministrationClient? _adminClient;
    private bool _isConnected;
    public bool IsConnected => _isConnected;

    private string _connectionString = string.Empty;
    public string ConnectionString => _connectionString;

    public async Task Connect(string connectionString)
    {
        _connectionString = connectionString;
        _adminClient = new ServiceBusAdministrationClient(connectionString);

        NamespaceProperties = await _adminClient.GetNamespacePropertiesAsync();


        var topics = new List<Topic>();
        await foreach (var serviceBusTopics in _adminClient.GetTopicsAsync())
        {
            var subs = new List<Subscription>();
            await foreach (var serviceBusSubscription in _adminClient.GetSubscriptionsAsync(serviceBusTopics.Name))
            {
                var rules = new List<RuleProperties>();
                await foreach (var serviceBusSubscriptionRules in _adminClient.GetRulesAsync(serviceBusTopics.Name,
                                   serviceBusSubscription.SubscriptionName))
                {
                    rules.Add(serviceBusSubscriptionRules);
                }

                subs.Add(new Subscription(
                    Name: serviceBusSubscription.SubscriptionName,
                    TopicName: serviceBusTopics.Name,
                    Properties: serviceBusSubscription,
                    // TODO: Figure out if we can fetch all and merge, to avoid too many calls
                    RuntimeProperties: await _adminClient.GetSubscriptionRuntimePropertiesAsync(serviceBusTopics.Name,
                        serviceBusSubscription.SubscriptionName),
                    Rules: rules
                ));
            }

            topics.Add(new Topic(
                Name: serviceBusTopics.Name,
                Properties: serviceBusTopics,
                // TODO: Figure out if we can fetch all and merge, to avoid too many calls
                RuntimeProperties: await _adminClient.GetTopicRuntimePropertiesAsync(serviceBusTopics.Name),
                Subscriptions: subs
            ));
        }

        TopicPageState = new Topics.PageState
        {
            Topics = topics
        };

        var queues = new List<Queue>();
        await foreach (var serviceBusQueue in _adminClient.GetQueuesAsync())
        {
            queues.Add(new Queue(
                Name: serviceBusQueue.Name,
                Properties: serviceBusQueue,
                // TODO: Figure out if we can fetch all and merge, to avoid too many calls
                RuntimeProperties: await _adminClient.GetQueueRuntimePropertiesAsync(serviceBusQueue.Name)
            ));
        }

        QueuePageState = new Queues.PageState
        {
            Queues = queues
        };

        _isConnected = true;
    }

    public void Disconnect()
    {
        _connectionString = string.Empty;
        _adminClient = null;
        _isConnected = false;
        NamespaceProperties = new NamespaceProperties();
        QueuePageState = new Queues.PageState { Queues = [] };
        TopicPageState = new Topics.PageState { Topics = [] };
    }

    public NamespaceProperties NamespaceProperties { get; private set; } = new();

    public Queues.PageState QueuePageState { get; private set; } = new() { Queues = [] };

    public Topics.PageState TopicPageState { get; private set; } = new() { Topics = [] };
}
