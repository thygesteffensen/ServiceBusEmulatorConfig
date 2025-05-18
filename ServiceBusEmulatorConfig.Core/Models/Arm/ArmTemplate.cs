namespace ServiceBusEmulatorConfig.Core.Models.Arm
{
    public class ArmTemplate
    {
        public string Schema { get; set; }
        public string ContentVersion { get; set; }
        public Dictionary<string, Parameter> Parameters { get; set; }
        public Dictionary<string, object> Variables { get; set; }
        public List<ArmResource> Resources { get; set; }
    }

    public class Parameter
    {
        public string DefaultValue { get; set; }
        public string Type { get; set; }
    }

    public class ArmResource
    {
        public string Type { get; set; }
        public string ApiVersion { get; set; }
        public string Name { get; set; }
        public string Location { get; set; }
        public List<string> DependsOn { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }
}
