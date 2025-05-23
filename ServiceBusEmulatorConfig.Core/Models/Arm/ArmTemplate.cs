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
    Dictionary<string, object> Properties
);
