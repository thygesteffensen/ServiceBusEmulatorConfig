using System.CommandLine;
using System.Diagnostics;

namespace ServiceBusEmulatorConfig.Cli.Commands;

public class WebCommand : Command
{
    public WebCommand() : base("web", "Transform Azure Service Bus ARM template to Emulator config")
    {
        this.SetHandler(async () => { await ExecuteAsync(); });
    }

    private async Task ExecuteAsync()
    {
        var process = new Process
        {
            StartInfo = new ProcessStartInfo
            {
                FileName =
                    "webapp/ServiceBusEmulatorConfig.Web",
                // WorkingDirectory = "webapp",
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            }
        };

        process.OutputDataReceived += (sender, args) =>
        {
            if (args.Data != null)
                Console.WriteLine(args.Data);
        };
        process.ErrorDataReceived += (sender, args) =>
        {
            if (args.Data != null)
                Console.Error.WriteLine(args.Data);
        };

        process.Start();
        process.BeginOutputReadLine();
        process.BeginErrorReadLine();
        await process.WaitForExitAsync();

        // return process.ExitCode;
    }
}
