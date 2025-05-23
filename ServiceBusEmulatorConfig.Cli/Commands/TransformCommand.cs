using System.CommandLine;
using ServiceBusEmulatorConfig.Core.Services;

namespace ServiceBusEmulatorConfig.Cli.Commands;

public class TransformCommand : Command
{
    public TransformCommand() : base("transform", "Transform Azure Service Bus ARM template to Emulator config")
    {
        var inputOption = new Option<FileInfo>(
            aliases: ["--input", "-i"],
            description: "Path to the ARM template JSON file")
        {
            IsRequired = true
        };

        var outputOption = new Option<FileInfo>(
            aliases: ["--output", "-o"],
            description: "Path to save the generated emulator config file")
        {
            IsRequired = true
        };

        var namespaceOption = new Option<string>(
            aliases: ["--namespace", "-n"],
            description: "The namespace name to use in the emulator config",
            getDefaultValue: () => "sbemulatorns");

        AddOption(inputOption);
        AddOption(outputOption);
        AddOption(namespaceOption);

        this.SetHandler(async (FileInfo input, FileInfo output, string namespaceName) =>
            {
                await ExecuteAsync(input, output, namespaceName);
            },
            inputOption, outputOption, namespaceOption);
    }

    private async Task ExecuteAsync(FileInfo input, FileInfo output, string namespaceName)
    {
        try
        {
            if (!input.Exists)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Error.WriteLineAsync($"Error: Input file not found: {input.FullName}");
                Console.ResetColor();
                Environment.Exit(1);
            }

            Console.WriteLine($"Transforming ARM template: {input.FullName}");
            Console.WriteLine($"Target namespace: {namespaceName}");

            var transformService = new TransformationService();
            var config = await transformService.TransformArmToEmulatorConfigAsync(input.FullName, namespaceName);
            var configJson = transformService.SerializeEmulatorConfig(config);
                
            // Ensure output directory exists
            output.Directory?.Create();

            await File.WriteAllTextAsync(output.FullName, configJson);
                
            Console.ForegroundColor = ConsoleColor.Green;
            Console.WriteLine($"Emulator config successfully written to: {output.FullName}");
            Console.ResetColor();
        }
        catch (Exception ex)
        {
            Console.ForegroundColor = ConsoleColor.Red;
            await Console.Error.WriteLineAsync($"Error during transformation: {ex.Message}");
            await Console.Error.WriteLineAsync(ex.StackTrace);
            Console.ResetColor();
            Environment.Exit(1);
        }
    }
}