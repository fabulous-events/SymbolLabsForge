using System;
using System.CommandLine;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SymbolLabsForge;
using SymbolLabsForge.Contracts;
using SymbolLabsForge.Services;
using SymbolLabsForge.Utils;
using SymbolLabsForge.CLI.Services;
using Microsoft.Extensions.Configuration;
using SymbolLabsForge.Configuration;

public class Program
{
    public static async Task<int> Main(string[] args)
    {
        var preliminaryServices = new ServiceCollection()
            .AddLogging()
            .AddSingleton<SessionManager>()
            .BuildServiceProvider();

        var sessionManager = preliminaryServices.GetRequiredService<SessionManager>();
        await sessionManager.LoadSessionAsync();

        var serviceProvider = new ServiceCollection()
            .AddLogging(builder => builder.AddConsole())
            .AddSymbolForge(new ConfigurationBuilder().Build()) // Pass a dummy configuration
            .AddTransient<CapsuleExporter>()
            .AddSingleton(provider => new CapsuleRegistryManager(Path.Combine(Directory.GetCurrentDirectory(), "..", "SymbolLabsForge.Docs", "CapsuleRegistry.json")))
            .AddSingleton(sessionManager) // Re-use the already created SessionManager
            .Configure<AssetSettings>(settings =>
            {
                settings.RootDirectory = "/mnt/e/ISP/Programs/Assets/";
                settings.Images = "Working/Images";
                settings.SandboxMode = sessionManager.CurrentState.SandboxMode;
            })
            .AddSingleton<IAssetPathProvider, AssetPathProvider>()
            .BuildServiceProvider();

        // --- Load Session ---
        Console.WriteLine($"[Session Loaded] Active Model: {sessionManager.CurrentState.ActiveModel ?? "Not Set"}, Sandbox Mode: {(sessionManager.CurrentState.SandboxMode ? "on" : "off")}");


        var rootCommand = new RootCommand("SymbolLabsForge CLI - The complete contributor toolchain.");

        // --- Build Command ---
        var buildCommand = new Command("build", "Builds the entire SymbolLabsForge solution.")
        {
            new Option<bool>("--all-phases", () => true, "Lock down and compile all modules.")
        };
        buildCommand.SetHandler(async () =>
        {
            if (sessionManager.CurrentState.SandboxMode)
            {
                Console.WriteLine("✅ [Sandbox Mode] Simulated execution of 'build' command.");
                return;
            }

            Console.WriteLine("--- Building all phases of SymbolLabsForge... ---");
            var solutionPath = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "SymbolLabsForge.sln"));
            var reportPath = "/mnt/e/ISP/Programs/SymbolLabsForge/SymbolLabsForge.Docs/ForgeBuildReport.md";
            
            var buildProcess = new System.Diagnostics.Process {
                StartInfo = new System.Diagnostics.ProcessStartInfo("dotnet", $"build \"{solutionPath}\"") { RedirectStandardOutput = true, UseShellExecute = false }
            };
            buildProcess.Start();
            string output = await buildProcess.StandardOutput.ReadToEndAsync();
            await buildProcess.WaitForExitAsync();

            if (buildProcess.ExitCode == 0) {
                Console.WriteLine("✅ Build Succeeded.");
                await File.WriteAllTextAsync(reportPath, $"# SymbolLabsForge - Master Build Report\n\n*   **Status**: ✅ Success\n*   **Timestamp**: {DateTime.UtcNow:o}\n\n**Build Output:**\n```\n{output}\n```");
            } else {
                Console.WriteLine("❌ Build Failed.");
                await File.WriteAllTextAsync(reportPath, $"# SymbolLabsForge - Master Build Report\n\n*   **Status**: ❌ Failure\n*   **Timestamp**: {DateTime.UtcNow:o}\n\n**Build Output:**\n```\n{output}\n```");
            }
        });
        rootCommand.AddCommand(buildCommand);

        // --- Test Command ---
        var testCommand = new Command("test", "Runs a suite of static and dynamic validation tests.")
        {
            new Option<bool>("--all", () => true, "Run all tests.")
        };
                    testCommand.SetHandler(RunAllTests);
        rootCommand.AddCommand(testCommand);
        
        // --- Model Command ---
        var modelArgument = new Argument<string>("modelName", "The name of the AI model to set as active.");
        var modelCommand = new Command("model", "Sets the active AI model.")
        {
            modelArgument
        };
        modelCommand.SetHandler(async (modelName) =>
        {
            sessionManager.CurrentState.ActiveModel = modelName;
            await sessionManager.SaveSessionAsync();
            Console.WriteLine($"Active model set to: {modelName}");
        }, modelArgument);
        rootCommand.AddCommand(modelCommand);

        // --- Sandbox Command ---
        var sandboxArgument = new Argument<string>("toggle", "Turn sandbox mode 'on' or 'off'.");
        var sandboxCommand = new Command("sandbox", "Enables or disables sandbox mode.")
        {
            sandboxArgument
        };
        sandboxCommand.SetHandler(async (toggle) =>
        {
            bool sandboxEnabled = toggle.ToLower() == "on";
            sessionManager.CurrentState.SandboxMode = sandboxEnabled;
            await sessionManager.SaveSessionAsync();
            var status = sandboxEnabled ? "on" : "off";
            Console.WriteLine($"Sandbox mode is now {status}.");

            var logFilePath = "/mnt/e/ISP/Programs/SymbolLabsForge/SymbolLabsForge.Docs/SessionComplianceLog.md";
            var logEntry = $@"
---
*   **Timestamp**: {DateTime.UtcNow:o}
*   **AuditTag**: `Command.SandboxToggle`

| Parameter          | Value                               |
|--------------------|-------------------------------------|
| **Command**        | `/sandbox`                          |
| **New State**      | `{status}`                          |
| **Rationale**      | `User toggled sandbox mode.`        |
";
            await File.AppendAllTextAsync(logFilePath, logEntry);
        }, sandboxArgument);
        rootCommand.AddCommand(sandboxCommand);

        // --- Image Command ---
        var imageArgument = new Argument<string>("fileName", "The name of the image file in the assets directory.");
        var outputOption = new Option<FileInfo>("--output-file", "The markdown file to append the image to.");
        var contextOption = new Option<string>("--context", "The validator context for logging.");
        var imageCommand = new Command("image", "Injects a base64 encoded image from the assets directory.")
        {
            imageArgument,
            outputOption,
            contextOption
        };
        imageCommand.SetHandler(async (fileName, outputFile, context) =>
        {
            var assetPathProvider = serviceProvider.GetRequiredService<IAssetPathProvider>();
            await InjectImage(assetPathProvider, fileName, outputFile, context);
        }, imageArgument, outputOption, contextOption);
        rootCommand.AddCommand(imageCommand);

        // --- All Other Commands ---
        AddPhaseCommands(rootCommand, serviceProvider);

        // --- Generate Replay Log Command ---
        var replayLogOutputOption = new Option<FileInfo>("--output-file", "The path to save the generated replay log.");
        var generateReplayLogCommand = new Command("generate-replay-log", "Generates a stub ReplayTraceLog.json file.")
        {
            replayLogOutputOption
        };
        generateReplayLogCommand.SetHandler(async (outputFile) =>
        {
            await GenerateReplayLog(outputFile);
        }, replayLogOutputOption);
        rootCommand.AddCommand(generateReplayLogCommand);

        // --- Audit Replay Bundle Command ---
        var auditFileInputOption = new Option<FileInfo>("--input-file", "The ReplayTraceLog.json file to audit.").ExistingOnly();
        var auditReplayBundleCommand = new Command("audit-replay-bundle", "Audits a replay bundle using a trace log.")
        {
            auditFileInputOption
        };
        auditReplayBundleCommand.SetHandler(async (inputFile) =>
        {
            await AuditReplayBundle(inputFile);
        }, auditFileInputOption);
        rootCommand.AddCommand(auditReplayBundleCommand);

        // --- Generate Synthetic Symbol Command ---
        var symbolTypeOption = new Option<MusicSymbolType>("--type", "The type of symbol to generate.");
        var strokeOption = new Option<float>("--stroke", () => 2.0f, "Stroke thickness.");
        var rotationOption = new Option<float>("--rotation", () => 0.0f, "Rotation in degrees.");
        var generateSyntheticCommand = new Command("generate-synthetic", "Generates a synthetic music symbol.")
        {
            symbolTypeOption,
            strokeOption,
            rotationOption
        };
        generateSyntheticCommand.SetHandler(async (symbolType, stroke, rotation) =>
        {
            var parameters = new SymbolParameters
            {
                SymbolType = symbolType,
                StrokeThickness = stroke,
                Rotation = rotation
            };
            await GenerateSyntheticSymbol(parameters);
        }, symbolTypeOption, strokeOption, rotationOption);
        rootCommand.AddCommand(generateSyntheticCommand);

        return await rootCommand.InvokeAsync(args);
    }

    private static void AddPhaseCommands(RootCommand rootCommand, IServiceProvider serviceProvider)
    {
        // --- Replay Test Command ---
        var replayCapsuleOption = new Option<FileInfo>("--capsule", "The path to the capsule's .json file.").ExistingOnly();
        var replayCommand = new Command("replay-test", "Replays a test from a capsule's .json file.") { replayCapsuleOption };
        replayCommand.SetHandler(async (file) => {
            var forge = serviceProvider.GetRequiredService<ISymbolForge>();
            await ReplayTest(forge, file.FullName);
        }, replayCapsuleOption);
        rootCommand.AddCommand(replayCommand);

        // --- Index Capsules Command ---
        var indexPathOption = new Option<DirectoryInfo>("--path", "The directory to scan for capsule .json files.").ExistingOnly();
        var indexCommand = new Command("index-capsules", "Finds all capsules and adds them to the registry.") { indexPathOption };
        indexCommand.SetHandler(async (path) => {
            var manager = serviceProvider.GetRequiredService<CapsuleRegistryManager>();
            await IndexCapsules(manager, path.FullName);
        }, indexPathOption);
        rootCommand.AddCommand(indexCommand);

        // --- Assemble Replay Bundle Command ---
        var assembleSymbolOption = new Option<SymbolType>("--symbol", "The symbol type to assemble a bundle for.");
        var assembleOutputOption = new Option<DirectoryInfo>("--output", "The directory to save the replay bundle.");
        var assembleCommand = new Command("assemble-replay", "Assembles a replay bundle.") { assembleSymbolOption, assembleOutputOption };
        assembleCommand.SetHandler(async (symbol, outputDir) => {
            await AssembleReplayBundle(symbol, outputDir.FullName);
        }, assembleSymbolOption, assembleOutputOption);
        rootCommand.AddCommand(assembleCommand);

        // --- Deploy Replay Bundle Command ---
        var deployBundleOption = new Option<string>("--bundle", "The name of the replay bundle to deploy.");
        var deployTargetOption = new Option<string>("--target", "The target pipeline to deploy to.");
        var deployCommand = new Command("deploy-replay", "Deploys a replay bundle.") { deployBundleOption, deployTargetOption };
        deployCommand.SetHandler((bundle, target) => Console.WriteLine($"✅ Simulated deployment of '{bundle}' to '{target}'. See DeploymentLog.md."), deployBundleOption, deployTargetOption);
        rootCommand.AddCommand(deployCommand);

        // --- Apply Fallback Command ---
        var fallbackCapsuleOption = new Option<string>("--capsule", "The capsule to apply fallback logic to.");
        var fallbackValidatorOption = new Option<string>("--validator", "The validator that failed.");
        var fallbackCommand = new Command("apply-fallback", "Applies fallback logic.") { fallbackCapsuleOption, fallbackValidatorOption };
        fallbackCommand.SetHandler((capsule, validator) => Console.WriteLine($"✅ Simulated fallback for '{capsule}' on failed '{validator}'. See FallbackMatrix.md."), fallbackCapsuleOption, fallbackValidatorOption);
        rootCommand.AddCommand(fallbackCommand);

        // --- Prioritize Capsules Command ---
        var prioBundleOption = new Option<string>("--bundle", "The bundle to prioritize.");
        var prioritizeCommand = new Command("prioritize-capsules", "Scores and prioritizes capsules.") { prioBundleOption };
        prioritizeCommand.SetHandler((bundle) => Console.WriteLine($"✅ Simulated prioritization for '{bundle}'. See CapsulePriority.md."), prioBundleOption);
        rootCommand.AddCommand(prioritizeCommand);
        
        // --- Schedule Validation Command ---
        var scheduleModeOption = new Option<string>("--mode", () => "auto", "Execution mode");
        var scheduleThresholdOption = new Option<string>("--threshold", () => "30d", "Stale threshold");
        var scheduleCommand = new Command("schedule-validation", "Autonomously schedules validation runs.") { scheduleModeOption, scheduleThresholdOption };
        scheduleCommand.SetHandler((mode, threshold) => Console.WriteLine($"✅ Simulated autonomous validation run. Stale threshold: {threshold}."), scheduleModeOption, scheduleThresholdOption);
        rootCommand.AddCommand(scheduleCommand);

        // --- Detect Evolution Command ---
        var evoCapsuleOption = new Option<string>("--capsule", "Optional specific capsule to check.");
        var detectEvolutionCommand = new Command("detect-evolution", "Detects capsules needing regeneration.") { evoCapsuleOption };
        detectEvolutionCommand.SetHandler((capsule) => Console.WriteLine($"✅ Simulated capsule evolution detection."), evoCapsuleOption);
        rootCommand.AddCommand(detectEvolutionCommand);

        // --- Generate Governance Artifacts Command ---
        var genGovModeOption = new Option<string>("--mode", () => "auto", "Execution mode");
        var generateGovCommand = new Command("generate-governance-artifacts", "Auto-generates onboarding and rationale docs.") { genGovModeOption };
        generateGovCommand.SetHandler((mode) => Console.WriteLine($"✅ Simulated generation of governance artifacts."), genGovModeOption);
        rootCommand.AddCommand(generateGovCommand);

        // --- Log Capsule Command ---
        var logCapsuleOption = new Option<string>("--capsule", "Capsule to log.");
        var logCapsuleCommand = new Command("log-capsule", "Logs a capsule to the immutable provenance ledger.") { logCapsuleOption };
        logCapsuleCommand.SetHandler((c) => Console.WriteLine($"✅ Simulated logging of '{c}' to CapsuleLedger.db."), logCapsuleOption);
        rootCommand.AddCommand(logCapsuleCommand);

        // --- Publish Capsule Command ---
        var pubCapsuleOption = new Option<string>("--capsule", "Capsule to publish.");
        var pubLicenseOption = new Option<string>("--license", "License for the capsule.");
        var publishCommand = new Command("publish-capsule", "Publishes a capsule to the public marketplace.") { pubCapsuleOption, pubLicenseOption };
        publishCommand.SetHandler((c, l) => Console.WriteLine($"✅ Simulated publishing of '{c}' with license '{l}'."), pubCapsuleOption, pubLicenseOption);
        rootCommand.AddCommand(publishCommand);

        // --- Detect Stubs Command ---
        var stubsBundleOption = new Option<string>("--bundle", "Bundle to scan for stubs.");
        var stubsCommand = new Command("detect-stubs", "Detects accidental stub patterns in a bundle.") { stubsBundleOption };
        stubsCommand.SetHandler((b) => Console.WriteLine($"✅ Simulated stub detection for '{b}'."), stubsBundleOption);
        rootCommand.AddCommand(stubsCommand);

        // --- Generate Teaching Guide Command ---
        var teachSymbolOption = new Option<string>("--symbol", "Symbol to generate a guide for.");
        var teachingCommand = new Command("generate-teaching-guide", "Auto-generates a teaching guide.") { teachSymbolOption };
        teachingCommand.SetHandler((s) => Console.WriteLine($"✅ Simulated teaching guide generation for '{s}'."), teachSymbolOption);
        rootCommand.AddCommand(teachingCommand);

        // --- Archive Capsule Command ---
        var archiveCapsuleOption = new Option<string>("--capsule", "Capsule to archive.");
        var archiveCommand = new Command("archive-capsule", "Archives a capsule to the immutable legacy archive.") { archiveCapsuleOption };
        archiveCommand.SetHandler((c) => Console.WriteLine($"✅ Simulated archival of '{c}'."), archiveCapsuleOption);
        rootCommand.AddCommand(archiveCommand);

        // --- Propose Governance Shift Command ---
        var govValidatorOption = new Option<string>("--validator", "Validator to propose a shift for.");
        var proposeGovCommand = new Command("propose-governance-shift", "Proposes a federation-wide governance shift.") { govValidatorOption };
        proposeGovCommand.SetHandler((v) => Console.WriteLine($"✅ Simulated governance proposal for '{v}'."), govValidatorOption);
        rootCommand.AddCommand(proposeGovCommand);

        // --- Launch Public Portal Command ---
        var launchConfigOption = new Option<string>("--config", "Launch checklist config file.");
        var launchCommand = new Command("launch-public-portal", "Executes the public launch checklist.") { launchConfigOption };
        launchCommand.SetHandler((c) => Console.WriteLine($"🚀 Simulated PUBLIC LAUNCH based on '{c}'."), launchConfigOption);
        rootCommand.AddCommand(launchCommand);
    }

    // TODO: Implement RunAllTests method
        private static async Task<int> RunAllTests()
        {
            Console.WriteLine(" --- Executing SymbolLabsForge Test Suite --- ");
            var solutionDir = Path.GetFullPath(Path.Combine(AppContext.BaseDirectory, "..", "..", "..", "..", "..")); // Adjust path to solution root
            var testProjectPath = Path.Combine(solutionDir, "Programs", "SymbolLabsForge", "SymbolLabsForge.Tests", "SymbolLabsForge.Tests.csproj");
            if (!File.Exists(testProjectPath)) { Console.Error.WriteLine($"Test project not found: {testProjectPath}"); return -1; }

            using var process = new System.Diagnostics.Process {
                StartInfo = new System.Diagnostics.ProcessStartInfo("dotnet", $"test \"{testProjectPath}\"") {
                    UseShellExecute = false, RedirectStandardOutput = true, RedirectStandardError = true, CreateNoWindow = true, WorkingDirectory = solutionDir
                }
            };
            process.OutputDataReceived += (s,e) => { if (e.Data != null) Console.WriteLine(e.Data); };
            process.ErrorDataReceived += (s,e) => { if (e.Data != null) { Console.Error.WriteLine(e.Data); } };
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            await process.WaitForExitAsync();
            return process.ExitCode;
        }

    // TODO: Implement InjectImage method
    private static async Task InjectImage(IAssetPathProvider assetPathProvider, string fileName, FileInfo? outputFile, string? context)
    {
        var imagePath = assetPathProvider.GetPath(fileName);
        if (!File.Exists(imagePath))
        {
            Console.Error.WriteLine($"Error: Image file not found at {imagePath}");
            return;
        }

        if (outputFile == null)
        {
            Console.Error.WriteLine("Error: Output file must be specified with --output-file.");
            return;
        }

        var imageBytes = await File.ReadAllBytesAsync(imagePath);
        var base64String = Convert.ToBase64String(imageBytes);
        var extension = Path.GetExtension(fileName).TrimStart('.');
        var markdownImage = $"![{Path.GetFileNameWithoutExtension(fileName)}](data:image/{extension};base64,{base64String})";

        await File.AppendAllTextAsync(outputFile.FullName, Environment.NewLine + markdownImage + Environment.NewLine);
        Console.WriteLine($"✅ Image '{fileName}' injected into '{outputFile.FullName}'.");

        // Log the injection
        var logFilePath = "/mnt/e/ISP/Programs/SymbolLabsForge/SymbolLabsForge.Docs/SessionComplianceLog.md";
        var logEntry = $@"
---
*   **Timestamp**: {DateTime.UtcNow:o}
*   **AuditTag**: `Command.InjectImage`

| Parameter          | Value                               |
|--------------------|-------------------------------------|
| **Command**        | `/image`                            |
| **File Name**      | `{fileName}`                        |
| **Output File**    | `{outputFile.FullName}`             |
| **Context**        | `{context ?? "N/A"}`                |
";
        await File.AppendAllTextAsync(logFilePath, logEntry);
    }

    // TODO: Implement ReplayTest method
    private static Task ReplayTest(ISymbolForge forge, string fullName)
    {
        // Implementation pending
        return Task.CompletedTask;
    }

    // TODO: Implement IndexCapsules method
    private static Task IndexCapsules(CapsuleRegistryManager manager, string fullName)
    {
        // Implementation pending
        return Task.CompletedTask;
    }

    // TODO: Implement AssembleReplayBundle method
    private static Task AssembleReplayBundle(SymbolType symbol, string fullName)
    {
        // Implementation pending
        return Task.CompletedTask;
    }

    // TODO: Implement GenerateReplayLog method
    private static async Task GenerateReplayLog(FileInfo outputFile)
    {
        Console.WriteLine($"--- Generating stub ReplayTraceLog.json at: {outputFile.FullName} ---");

        var log = new ReplayTraceLog
        {
            BundleId = "TestBundle-001",
            Timestamp = DateTime.UtcNow,
            Events = new List<ReplayEvent>
            {
                new ReplayEvent
                {
                    CapsuleId = "Capsule-Flat-01",
                    SymbolType = "Flat",
                    ValidatorOutcomes = new List<ValidatorOutcome>
                    {
                        new ValidatorOutcome { ValidatorName = "ContrastValidator", Outcome = "Pass", Confidence = 0.95f, Rationale = "High contrast detected." },
                        new ValidatorOutcome { ValidatorName = "DensityValidator", Outcome = "Pass", Confidence = 0.88f, Rationale = "Density within acceptable range." },
                        new ValidatorOutcome { ValidatorName = "StructureValidator", Outcome = "Pass", Confidence = 0.92f, Rationale = "Structure is valid." }
                    },
                    ArbitrationDecision = new ArbitrationDecision { FinalOutcome = "Pass", WinningValidator = "Consensus", Reason = "All validators passed." }
                },
                new ReplayEvent
                {
                    CapsuleId = "Capsule-Sharp-02",
                    SymbolType = "Sharp",
                    ValidatorOutcomes = new List<ValidatorOutcome>
                    {
                        new ValidatorOutcome { ValidatorName = "ContrastValidator", Outcome = "Pass", Confidence = 0.91f, Rationale = "High contrast detected." },
                        new ValidatorOutcome { ValidatorName = "DensityValidator", Outcome = "Fail", Confidence = 0.45f, Rationale = "Density too low." },
                        new ValidatorOutcome { ValidatorName = "StructureValidator", Outcome = "Override", Confidence = 0.99f, Rationale = "Structure is valid despite low density." }
                    },
                    ArbitrationDecision = new ArbitrationDecision { FinalOutcome = "Pass", WinningValidator = "StructureValidator", Reason = "Override due to high confidence in structure." }
                }
            }
        };

        var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
        var json = System.Text.Json.JsonSerializer.Serialize(log, options);
        await File.WriteAllTextAsync(outputFile.FullName, json);

        Console.WriteLine("✅ Successfully generated stub replay log.");
    }

    // TODO: Implement AuditReplayBundle method
    private static async Task AuditReplayBundle(FileInfo inputFile)
    {
        Console.WriteLine($"--- Auditing replay bundle from: {inputFile.FullName} ---");

        var json = await File.ReadAllTextAsync(inputFile.FullName);
        var log = System.Text.Json.JsonSerializer.Deserialize<ReplayTraceLog>(json);

        var auditReport = new System.Text.StringBuilder();
        auditReport.AppendLine($"# Replay Bundle Audit Report for {log.BundleId}");
        auditReport.AppendLine($"*   **Timestamp**: {DateTime.UtcNow:o}");
        auditReport.AppendLine();

        int discrepancies = 0;

        foreach (var evt in log.Events)
        {
            auditReport.AppendLine($"## Auditing Capsule: {evt.CapsuleId}");

            // Simulate arbitration logic
            var overrideValidator = evt.ValidatorOutcomes.FirstOrDefault(v => v.Outcome == "Override");
            string simulatedOutcome;
            string simulatedReason;

            if (overrideValidator != null)
            {
                simulatedOutcome = "Pass";
                simulatedReason = $"Override by {overrideValidator.ValidatorName} with confidence {overrideValidator.Confidence}.";
            }
            else if (evt.ValidatorOutcomes.All(v => v.Outcome == "Pass"))
            {
                simulatedOutcome = "Pass";
                simulatedReason = "Consensus: All validators passed.";
            }
            else
            {
                simulatedOutcome = "Fail";
                simulatedReason = "Consensus: One or more validators failed without an override.";
            }

            // Compare with logged decision
            if (simulatedOutcome == evt.ArbitrationDecision.FinalOutcome)
            {
                auditReport.AppendLine("*   **Status**: ✅ Matched");
                auditReport.AppendLine($"*   **Logged**: `{evt.ArbitrationDecision.FinalOutcome}` | **Simulated**: `{simulatedOutcome}`");
                auditReport.AppendLine($"*   **Rationale**: {simulatedReason}");
            }
            else
            {
                discrepancies++;
                auditReport.AppendLine("*   **Status**: ❌ Discrepancy Found");
                auditReport.AppendLine($"*   **Logged**: `{evt.ArbitrationDecision.FinalOutcome}` | **Simulated**: `{simulatedOutcome}`");
                auditReport.AppendLine($"*   **Logged Reason**: {evt.ArbitrationDecision.Reason}");
                auditReport.AppendLine($"*   **Simulated Rationale**: {simulatedReason}");
            }
            auditReport.AppendLine();
        }

        var auditFilePath = "ReplayBundleAudit.md";
        await File.WriteAllTextAsync(auditFilePath, auditReport.ToString());
        Console.WriteLine($"✅ Audit complete. Report generated at {auditFilePath}");

        // Log to SessionComplianceLog
        var logFilePath = "/mnt/e/ISP/Programs/SymbolLabsForge/SymbolLabsForge.Docs/SessionComplianceLog.md";
        var logEntry = $@"
---
*   **Timestamp**: {DateTime.UtcNow:o}
*   **AuditTag**: `Task.ReplayBundleAudit`

| Task                               | Status      | Notes                                                                                                                                                           |
|------------------------------------|-------------|-----------------------------------------------------------------------------------------------------------------------------------------------------------------|
| **Execute Replay Bundle Audit**    | ✅ Complete   | Audited `{log.BundleId}`. Found {discrepancies} discrepancies. See `ReplayBundleAudit.md` for details. |
";
        await File.AppendAllTextAsync(logFilePath, logEntry);
    }

    // TODO: Implement GenerateSyntheticSymbol method
    private static async Task GenerateSyntheticSymbol(SymbolParameters parameters)
    {
        Console.WriteLine($"--- Generating synthetic symbol: {parameters.SymbolType} ---");

        var generator = new SymbolLabsForge.Generators.SyntheticSymbolGenerator();
        var dimensions = new SixLabors.ImageSharp.Size(128, 128);

        // 1. Generate (now includes skeletonization)
        var finalImage = generator.Generate(parameters, dimensions);

        // 2. Save output
        var outputDir = "/mnt/e/ISP/Programs/Assets/SyntheticSymbols/";
        var timestamp = DateTime.UtcNow.ToString("yyyyMMddHHmmssfff");
        var baseFileName = $"{parameters.SymbolType}_{timestamp}";
        
        var imagePath = Path.Combine(outputDir, $"{baseFileName}.png");
        using (var stream = File.Create(imagePath))
        {
            await finalImage.SaveAsync(stream, new SixLabors.ImageSharp.Formats.Png.PngEncoder());
        }

        var metadataPath = Path.Combine(outputDir, $"{baseFileName}.json");
        var options = new System.Text.Json.JsonSerializerOptions { WriteIndented = true };
        var json = System.Text.Json.JsonSerializer.Serialize(parameters, options);
        await File.WriteAllTextAsync(metadataPath, json);

        Console.WriteLine($"✅ Successfully generated symbol and metadata at {outputDir}");
    }
}
