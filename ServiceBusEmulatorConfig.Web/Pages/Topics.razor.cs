using ServiceBusEmulatorConfig.Core.Models.ServiceBus;

namespace ServiceBusEmulatorConfig.Web.Pages;

public partial class Topics
{
    private List<ServiceBusTopic>? _topics;
    private bool _loading;
    private string? _error;
    private Identifier? _selectedNodeId;
    private ServiceBusTopic? _selectedTopic;
    private ServiceBusSubscription? _selectedSubscription;

    protected override async Task OnInitializedAsync()
    {
        if (ConnectionService.IsConnected)
        {
            await LoadTopicsAsync();
        }
    }

    private async Task LoadTopicsAsync()
    {
        try
        {
            _loading = true;
            _error = null;
            _topics = await ConnectionService.GetTopicsAsync();
        }
        catch (Exception ex)
        {
            _error = ex.Message;
        }
        finally
        {
            _loading = false;
        }
    }

    private void OnSelectedValueChanged(Identifier identifier)
    {
        _selectedNodeId = identifier;

        if (_selectedNodeId == null)
        {
            _selectedTopic = null;
            _selectedSubscription = null;
            return;
        }

        if (_selectedNodeId is TopicIdentifier topicIdentifier)
        {
            _selectedTopic = _topics?.FirstOrDefault(t => t.Name == topicIdentifier.TopicName);
            _selectedSubscription = null;
        }
        else if (_selectedNodeId is SubscriptionIdentifier subscriptionIdentifier)
        {
            _selectedTopic = _topics?.FirstOrDefault(t => t.Name == subscriptionIdentifier.TopicName);

            if (_selectedTopic != null)
            {
                _selectedSubscription =
                    _selectedTopic.Subscriptions.FirstOrDefault(s => s.Name == subscriptionIdentifier.SubscriptionName);
            }
        }
    }
}
