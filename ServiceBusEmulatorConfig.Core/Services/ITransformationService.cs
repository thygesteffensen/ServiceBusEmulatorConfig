using System.Threading.Tasks;
using ServiceBusEmulatorConfig.Core.Models.Emulator;

namespace ServiceBusEmulatorConfig.Core.Services
{
    public interface ITransformationService
    {
        /// <summary>
        /// Transforms Azure Service Bus ARM template JSON into Emulator configuration
        /// </summary>
        /// <param name="armJson">The ARM template JSON content</param>
        /// <param name="namespaceName">The target namespace name for the emulator</param>
        /// <returns>Emulator configuration object</returns>
        EmulatorConfig TransformArmToEmulatorConfig(string armJson, string namespaceName);

        /// <summary>
        /// Transforms Azure Service Bus ARM template from file into Emulator configuration
        /// </summary>
        /// <param name="armJsonPath">Path to the ARM template JSON file</param>
        /// <param name="namespaceName">The target namespace name for the emulator</param>
        /// <returns>Emulator configuration object</returns>
        Task<EmulatorConfig> TransformArmToEmulatorConfigAsync(string armJsonPath, string namespaceName);

        /// <summary>
        /// Serializes the Emulator configuration to JSON
        /// </summary>
        /// <param name="config">The Emulator configuration object</param>
        /// <returns>JSON representation of the configuration</returns>
        string SerializeEmulatorConfig(EmulatorConfig config);
    }
}
