using ServiceBusEmulatorConfig.Web.Model;
using ServiceBusEmulatorConfig.Web.State;

namespace ServiceBusEmulatorConfig.Web.Pages;

public partial class Queues(
    IApplicationState applicationState
)
{
    public class PageState
    {
        public List<Queue> Queues { get; init; } = [];
    }

    private readonly PageState _pageState = applicationState.QueuePageState;

    private Queue? _selectedQueue;
    private string _searchString = "";

    private bool FilterFunc(Queue queue)
    {
        if (string.IsNullOrWhiteSpace(_searchString))
            return true;

        return queue.Name.Contains(_searchString, StringComparison.OrdinalIgnoreCase);
    }

    private void SelectQueue(Queue queue)
    {
        _selectedQueue = queue;
    }
}
