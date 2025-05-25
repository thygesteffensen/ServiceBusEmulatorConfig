using ServiceBusEmulatorConfig.Web.Model;
using ServiceBusEmulatorConfig.Web.State;

namespace ServiceBusEmulatorConfig.Web.Pages;

public partial class Topics(IApplicationState applicationState)
{
    public class PageState
    {
        public List<Topic> Topics { get; set; } = [];
    }

    private readonly PageState _pageState = applicationState.TopicPageState;

    private Identifier? _selectedNodeId;
    private Topic? _selectedTopic;
    private Subscription? _selectedSubscription;
    private string _searchString = "";
    
    private bool FilterTopic(Topic topic)
    {
        if (string.IsNullOrWhiteSpace(_searchString))
            return true;
            
        // Check if topic name matches
        if (topic.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase))
            return true;
            
        // Check if any subscription name matches
        return topic.Subscriptions.Any(sub => sub.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase));
    }

    private void OnSelectedValueChanged(Identifier identifier)
    {
        _selectedNodeId = identifier;

        switch (_selectedNodeId)
        {
            case null:
                _selectedTopic = null;
                _selectedSubscription = null;
                return;
            case TopicIdentifier topicIdentifier:
                _selectedTopic = _pageState.Topics.FirstOrDefault(t => t.Name == topicIdentifier.TopicName);
                _selectedSubscription = null;
                break;
            case SubscriptionIdentifier subscriptionIdentifier:
            {
                _selectedTopic = _pageState.Topics.FirstOrDefault(t => t.Name == subscriptionIdentifier.TopicName);

                if (_selectedTopic != null)
                {
                    _selectedSubscription =
                        _selectedTopic.Subscriptions.FirstOrDefault(s => s.Name == subscriptionIdentifier.SubscriptionName);
                }

                break;
            }
        }
    }
}
