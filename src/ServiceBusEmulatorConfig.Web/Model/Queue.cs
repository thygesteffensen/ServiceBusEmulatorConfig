using Azure.Messaging.ServiceBus.Administration;

namespace ServiceBusEmulatorConfig.Web.Model;

public record Queue(string Name, QueueProperties Properties, QueueRuntimeProperties RuntimeProperties);
