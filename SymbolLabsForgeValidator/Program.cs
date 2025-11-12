using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using SymbolLabsForge;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Utils;
using Newtonsoft.Json;
using System.CommandLine;

class Program
{
    static async Task<int> Main(string[] args)
    {
        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole().SetMinimumLevel(LogLevel.Warning))
            .AddSymbolForge()
            .BuildServiceProvider();


        var rootCommand = new RootCommand("SymbolLabsForge Capsule Validator");

        var capsuleArgument = new Argument<FileInfo>(
            name: "capsule",
            description: "Path to the capsule's .json file.").ExistingOnly();

        rootCommand.AddArgument(capsuleArgument);

        var aiAssistOption = new Option<bool>(
            name: "--ai-assist",
            description: "Enable AI-assisted validation arbitration.",
            getDefaultValue: () => false);

        rootCommand.AddOption(aiAssistOption);

        rootCommand.SetHandler(async (fileInfo, aiAssist) =>
        {
            if (aiAssist)
            {
                var arbitrator = serviceProvider.GetRequiredService<SymbolLabsForge.Validation.AI.IAIValidatorArbitrator>();
                await AidedValidation(arbitrator, fileInfo.FullName);
            }
            else
            {
                var forge = serviceProvider.GetRequiredService<ISymbolForge>();
                await ValidateCapsule(forge, fileInfo.FullName);
            }
        }, capsuleArgument, aiAssistOption);

        return await rootCommand.InvokeAsync(args);
    }

    private static async Task AidedValidation(SymbolLabsForge.Validation.AI.IAIValidatorArbitrator arbitrator, string capsulePath)
    {
        Console.WriteLine($"--- AI-Assisted Validation for: {capsulePath} ---");
        var (capsule, _) = await CapsuleLoader.LoadFromFileAsync(capsulePath);

        // Simulate AI validator results
        var claudeResult = new ValidationResult(true, "Claude Validator", "Density matches known-good morph lineage.");
        var vortexResult = new ValidationResult(false, "Vortex Validator", "Stroke width exceeds typeset threshold.");

        var arbitrationResult = arbitrator.Arbitrate(capsule, claudeResult, vortexResult);

        Console.WriteLine(JsonConvert.SerializeObject(arbitrationResult, Formatting.Indented));
    }

    private static async Task ValidateCapsule(ISymbolForge forge, string capsulePath)
    {
        Console.WriteLine($"--- Validating: {capsulePath} ---");
        try
        {
            var (capsule, request) = await CapsuleLoader.LoadFromFileAsync(capsulePath);
            
            // Re-run validation
            var regeneratedCapsule = forge.Generate(request).Primary;

            if (regeneratedCapsule.IsValid)
            {
                Console.WriteLine("✅ PASS: Capsule is valid.");
                Environment.Exit(0);
            }
            else
            {
                Console.WriteLine("❌ FAIL: Capsule is invalid.");
                foreach (var result in regeneratedCapsule.ValidationResults.Where(r => !r.IsValid))
                {
                    Console.WriteLine($"  - [{result.ValidatorName}]: {result.FailureMessage}");
                }
                Environment.Exit(1);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"An unexpected error occurred: {ex.Message}");
            Environment.Exit(-1);
        }
    }
}
