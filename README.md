# Tools to work efficiently with the Azure Service Bus [Emulator] (Work in progress)

## Features
- Convert Azure Service Bus Namespace ARM template to Azure Service Bus emulator configuration.
- Inspect Azure Service Bus Namespace
  - See Topic, subscriptions and rules
  - See Queues

### Next features
- Peak messages
- Send messages

## The tool

Install the NuGet.org version as: `$ dotnet tool install --global ServiceBusEmulatorConfig.Cli`, it is invoked by `$ sbconfig`.

**Install, or update, from source:**

1. `$ pushd ServiceBusEmulatorConfig.Cli`
2. `$ dotnet pack -c Release -p:PackageVersion 1.0.1 `
3. `$ dotnet tool install -g SerbiceBusEumlatorConfig.Cli --add-source nupkg/`

### ARM to emulator config
The ARM template must be manually downloaded or compiled using `$ az bicep build --file <bicep.bicep>`
```sh
$ sbconfig transform --help
Description:
  Transform Azure Service Bus ARM template to Emulator config

Usage:
  ServiceBusEmulatorConfig.Cli transform [options]

Options:
  -i, --input <input> (REQUIRED)    Path to the ARM template JSON file
  -o, --output <output> (REQUIRED)  Path to save the generated emulator config file
  -n, --namespace <namespace>       The namespace name to use in the emulator config [default: sbemulatorns]
  -?, -h, --help                    Show help and usage information
```

#### Aspire usage

Mount the config file:
```cs
var serviceBus = builder
    .AddAzureServiceBus("esb")
    .RunAsEmulator(em =>
    {
        em.WithBindMount("Config.json", "/ServiceBus_Emulator/ConfigFiles/Config.json");
    });
```
_NB: This runs as docker, so the `Config.json` must be at the root, or below, of the Aspire project._

#### Docker usage
Given the [example](https://learn.microsoft.com/en-us/azure/service-bus-messaging/test-locally-with-service-bus-emulator?tabs=docker-linux-container) from Microsoft, specify the path to the outputted `Config.json` in the `.env` file. _Remember, rhis runs as docker, so the `Config.json` must be at the root, or below, of the Aspire project._

### Inspect Azure Service Bus Namespace
Until [Azure/azure-service-bus-emulator-installer/issues/17](https://github.com/Azure/azure-service-bus-emulator-installer/issues/17) have been resolved, we cannot use the explorer (or any viwer) for the emulator...

~~But the explorer is cross platform, and can be launched as `$ sbconfig explorer`.~~

~~This launched as local web api and the explorer is simply a webinterface - crossplatform without hazzle :)~~

For now, the repo must be cloned and executed manually.
 
## Contribute

Feel free to open an issue or PR! 🚀