using System.CommandLine;
using ServiceBusEmulatorConfig.Cli.Commands;

namespace ServiceBusEmulatorConfig.Cli
{
    class Program
    {
        static async Task<int> Main(string[] args)
        {
            var rootCommand = new RootCommand("Azure Service Bus Emulator Configuration Tool");
            
            // Add transform command
            rootCommand.AddCommand(new TransformCommand());

            // Set description for the root command
            rootCommand.Description = "Tool to transform Azure Service Bus ARM templates to emulator configuration format";

            try
            {
                return await rootCommand.InvokeAsync(args);
            }
            catch (Exception ex)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                await Console.Error.WriteLineAsync($"Unhandled exception: {ex.Message}");
                await Console.Error.WriteLineAsync(ex.StackTrace);
                Console.ResetColor();
                return 1;
            }
        }
    }
}
