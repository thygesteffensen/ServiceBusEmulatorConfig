using ServiceBusEmulatorConfig.SDK;
using ServiceBusEmulatorConfig.SDK.Models;
using ServiceBusEmulatorConfig.Core.Models.Emulator;

namespace ServiceBusEmulatorConfig.Web.Services
{
    public class ServiceBusConnectionService
    {
        private ServiceBusExplorerClient? _client;
        
        public bool IsConnected => _client != null;
        public string ConnectionString { get; private set; } = string.Empty;

        public async Task ConnectAsync(string connectionString)
        {
            if (string.IsNullOrWhiteSpace(connectionString))
            {
                throw new ArgumentException("Connection string cannot be empty", nameof(connectionString));
            }

            try
            {
                _client = new ServiceBusExplorerClient(connectionString);
                ConnectionString = connectionString;
                
                // Validate connection by getting namespace details
                await GetNamespaceDetailsAsync();
            }
            catch (Exception ex)
            {
                _client = null;
                ConnectionString = string.Empty;
                throw new Exception($"Failed to connect to Service Bus: {ex.Message}", ex);
            }
        }

        public async Task<ServiceBusNamespace> GetNamespaceDetailsAsync()
        {
            if (!IsConnected || _client == null)
            {
                throw new InvalidOperationException("Not connected to any Service Bus namespace");
            }

            return await _client.GetNamespaceDetailsAsync();
        }

        public async Task<List<ServiceBusQueue>> GetQueuesAsync()
        {
            if (!IsConnected || _client == null)
            {
                throw new InvalidOperationException("Not connected to any Service Bus namespace");
            }

            return await _client.GetQueuesAsync();
        }

        public async Task<List<ServiceBusTopic>> GetTopicsAsync()
        {
            if (!IsConnected || _client == null)
            {
                throw new InvalidOperationException("Not connected to any Service Bus namespace");
            }

            return await _client.GetTopicsAsync();
        }

        public async Task<List<ServiceBusSubscription>> GetSubscriptionsAsync(string topicName)
        {
            if (!IsConnected || _client == null)
            {
                throw new InvalidOperationException("Not connected to any Service Bus namespace");
            }

            if (string.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentException("Topic name cannot be empty", nameof(topicName));
            }

            return await _client.GetSubscriptionsAsync(topicName);
        }

        public async Task<List<ServiceBusRule>> GetRulesAsync(string topicName, string subscriptionName)
        {
            if (!IsConnected || _client == null)
            {
                throw new InvalidOperationException("Not connected to any Service Bus namespace");
            }

            if (string.IsNullOrWhiteSpace(topicName))
            {
                throw new ArgumentException("Topic name cannot be empty", nameof(topicName));
            }

            if (string.IsNullOrWhiteSpace(subscriptionName))
            {
                throw new ArgumentException("Subscription name cannot be empty", nameof(subscriptionName));
            }

            return await _client.GetRulesAsync(topicName, subscriptionName);
        }

        public async Task<EmulatorConfig> ExportConfigAsync()
        {
            if (!IsConnected || _client == null)
            {
                throw new InvalidOperationException("Not connected to any Service Bus namespace");
            }

            return await _client.ExportToEmulatorConfigAsync();
        }

        public void Disconnect()
        {
            _client = null;
            ConnectionString = string.Empty;
        }
    }
}
