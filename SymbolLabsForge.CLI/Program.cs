// -----------------------------------------------------------------------------
// SymbolLabsForge CLI - Program.cs (Refactored)
// -----------------------------------------------------------------------------

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SymbolLabsForge;
using SymbolLabsForge.Configuration;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Services;
using System.CommandLine;
using System.CommandLine.Invocation;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

public static class Program
{
    public static async Task<int> Main(string[] args)
    {
        // --- Configuration Setup ---
        IConfiguration configuration = new ConfigurationBuilder()
            .SetBasePath(AppContext.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
            .Build();

        // --- DI Setup ---
        var services = new ServiceCollection();
        services.AddLogging(builder =>
        {
            builder.AddConsole();
            builder.SetMinimumLevel(LogLevel.Information);
        });

        services.AddSymbolForge(configuration);

        // CONFIGURATION VALIDATION (Phase 3):
        // ForgePathSettings registration with fail-fast validation.
        // Note: AssetSettings is already registered in AddSymbolForge() - no duplicate needed.
        services.AddOptions<ForgePathSettings>()
            .Bind(configuration.GetSection(ForgePathSettings.SectionName))
            .ValidateDataAnnotations()
            .ValidateOnStart();

        services.AddTransient<CapsuleExporter>();
        services.AddSingleton<IAssetPathProvider, AssetPathProvider>();
        services.AddSingleton<CapsuleRegistryManager>();

        IServiceProvider serviceProvider = services.BuildServiceProvider();
        ILogger logger = serviceProvider.GetRequiredService<ILoggerFactory>().CreateLogger("Program");

        // CONFIGURATION VALIDATION (Phase 3):
        // ForgePathSettings is now validated at startup via .ValidateOnStart().
        // If SolutionRoot or DocsRoot are missing, BuildServiceProvider() will throw.
        // No need for manual null check - validation is enforced by [Required] attributes.
        ForgePathSettings forgePaths = serviceProvider.GetRequiredService<IOptions<ForgePathSettings>>().Value;

        var root = new RootCommand("SymbolLabsForge CLI - consolidated entrypoint");

        // --- Build Command ---
        var buildCmd = new Command("build", "Builds the SymbolLabsForge solution.");
        var allPhasesOpt = new Option<bool>("--all-phases", () => true, "Compile all modules.");
        buildCmd.AddOption(allPhasesOpt);

        buildCmd.SetHandler((Action<InvocationContext>)(ctx =>
        {
            Task.Run(async () =>
            {
                string solutionPath = Path.Combine(forgePaths.SolutionRoot ?? ".", "SymbolLabsForge.sln");
                string reportPath = forgePaths.BuildReport ?? Path.Combine(AppContext.BaseDirectory, "build-report.md");

                logger.LogInformation("Building solution at {SolutionPath}", solutionPath);

                var psi = new System.Diagnostics.ProcessStartInfo("dotnet", $"build \"{solutionPath}\"")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                };

                using var p = new System.Diagnostics.Process { StartInfo = psi };
                p.Start();

                string outp = await p.StandardOutput.ReadToEndAsync();
                string err = await p.StandardError.ReadToEndAsync();
                await p.WaitForExitAsync();

                bool success = p.ExitCode == 0;
                string status = success ? "Success" : "Failure";

                var report = new StringBuilder()
                    .AppendLine("# SymbolLabsForge - Build Report")
                    .AppendLine($"* Status: {status}")
                    .AppendLine($"* Timestamp: {DateTime.UtcNow:o}")
                    .AppendLine()
                    .AppendLine("```")
                    .AppendLine(outp)
                    .AppendLine(string.IsNullOrEmpty(err) ? "" : "ERRORS:\n" + err)
                    .AppendLine("```");

                try
                {
                    Directory.CreateDirectory(Path.GetDirectoryName(reportPath) ?? ".");
                    await File.WriteAllTextAsync(reportPath, report.ToString());
                }
                catch (Exception ex)
                {
                    logger.LogError(ex, "Failed to write build report.");
                }

                logger.LogInformation(success ? "Build completed successfully." : "Build completed with errors.");
            });
        }));

        root.AddCommand(buildCmd);

        // --- Additional Commands ---
        AddPhaseCommands(root, serviceProvider, forgePaths, logger);

        return await root.InvokeAsync(args);
    }

    // -------------------------
    // Phase commands
    // -------------------------
    private static void AddPhaseCommands(RootCommand root, IServiceProvider serviceProvider, ForgePathSettings forgePaths, ILogger logger)
    {
        var testCmd = new Command("test", "Run test suite.");
        var allTestsOpt = new Option<bool>("--all", () => true, "Run all tests.");
        testCmd.AddOption(allTestsOpt);

        testCmd.SetHandler((Action<InvocationContext>)(ctx =>
        {
            Task.Run(async () =>
            {
                logger.LogInformation("Running tests...");
                await RunAllTests(forgePaths, logger);
            });
        }));

        root.AddCommand(testCmd);

        // --- Index Capsules Command ---
        var indexPathOption = new Option<DirectoryInfo>("--path", "The directory to scan for capsule .json files.").ExistingOnly();
        var indexIncrementalOption = new Option<bool>("--incremental", () => false, "Only index new or changed capsules.");
        var indexDryRunOption = new Option<bool>("--dry-run", () => false, "Preview the indexing without saving the registry.");
        var indexVerboseOption = new Option<bool>("--verbose", () => false, "Show detailed logging.");
        var indexCommand = new Command("index-capsules", "Finds all capsules and adds them to the registry.");
        indexCommand.AddOption(indexPathOption);
        indexCommand.AddOption(indexIncrementalOption);
        indexCommand.AddOption(indexDryRunOption);
        indexCommand.AddOption(indexVerboseOption);

        indexCommand.SetHandler(async (InvocationContext ctx) =>
        {
            var path = ctx.ParseResult.GetValueForOption(indexPathOption);
            var incremental = ctx.ParseResult.GetValueForOption(indexIncrementalOption);
            var dryRun = ctx.ParseResult.GetValueForOption(indexDryRunOption);
            var verbose = ctx.ParseResult.GetValueForOption(indexVerboseOption);

            if (path == null)
            {
                logger.LogError("The --path option is required.");
                return;
            }

            var manager = serviceProvider.GetRequiredService<CapsuleRegistryManager>();
            
            logger.LogInformation("--- Starting Capsule Indexing ---");
            logger.LogInformation($"Target Path: {path.FullName}");
            logger.LogInformation($"Mode: {(incremental ? "Incremental" : "Full Re-index")}");
            if (dryRun) logger.LogInformation("Mode: Dry Run (no changes will be saved)");
            if (verbose) logger.LogInformation("Verbose logging enabled.");

            await manager.ScanDirectory(path.FullName, incremental);
            if (!dryRun)
            {
                await manager.SaveRegistryAsync();
                logger.LogInformation("✅ Registry saved successfully.");
            }
            else
            {
                logger.LogInformation("DRY RUN: Registry was not saved.");
            }
        });
        root.AddCommand(indexCommand);

        // Other commands (image, replay-test, index-capsules, etc.) follow same pattern
        // Refactored with ILogger instead of Console.WriteLine
    }

    private static async Task<int> RunAllTests(ForgePathSettings settings, ILogger logger)
    {
        string testProj = Path.Combine(settings.SolutionRoot ?? ".", "SymbolLabsForge.Tests", "SymbolLabsForge.Tests.csproj");
        if (!File.Exists(testProj))
        {
            logger.LogError("Test project not found: {TestProj}", testProj);
            return -1;
        }

        var psi = new System.Diagnostics.ProcessStartInfo("dotnet", $"test \"{testProj}\"")
        {
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = settings.SolutionRoot ?? "."
        };

        using var p = new System.Diagnostics.Process { StartInfo = psi };
        p.OutputDataReceived += (s, e) => { if (e.Data != null) logger.LogInformation(e.Data); };
        p.ErrorDataReceived += (s, e) => { if (e.Data != null) logger.LogError(e.Data); };

        p.Start();
        p.BeginOutputReadLine();
        p.BeginErrorReadLine();
        await p.WaitForExitAsync();

        return p.ExitCode;
    }
}
