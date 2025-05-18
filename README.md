# Tools to work efficiently with the Azure Service Bus

## ARM to emulator config

To install or update the tool from souce:
1. `pushd ServiceBusEmulatorConfig.Cli`
2. `dotnet package -c Release -p:PackageVersion 1.0.1 `
3. `dotnet tool install -g SerbiceBusEumlatorConfig.Tool --add-source nupkg/`
