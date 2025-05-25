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
