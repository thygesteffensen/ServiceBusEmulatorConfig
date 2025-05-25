namespace ServiceBusEmulatorConfig.Core.Models.Arm;

public record ArmTemplate(
    string Schema,
    string ContentVersion,
    Dictionary<string, Parameter> Parameters,
    Dictionary<string, object> Variables,
    List<ArmResource> Resources
);

public record Parameter(
    string DefaultValue,
    string Type
);

public record ArmResource(
    string Type,
    string ApiVersion,
    string Name,
    string Location,
    List<string> DependsOn,
    ArmResourceProperties Properties
);

public record ArmResourceProperties(
    string LockDuration = "PT5M",
    int MaxDeliveryCount = 10,
    string? ForwardDeadLetteredMessagesTo = null,
    string? ForwardTo = null,
    bool RequiresSession = false,
    bool DeadLetteringOnMessageExpiration = false,
    bool RequiresDuplicateDetection = false,
    string? FilterType = null,
    SqlFilter? SqlFilter = null,
    SqlFilter? Action = null
);

public record SqlFilter(
    string SqlExpression,
    int CompatabilityLevel = 20
);
