using BenchmarkDotNet.Running;
using SymbolLabsForge.Benchmarks.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        // Phase 7: Comprehensive performance benchmarking suite
        // Validates that Phase 5 (provenance tracking) and Phase 6 (UI refactor)
        // did not introduce performance regressions.

        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("SymbolLabsForge - Phase 7 Performance Benchmarking Suite");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();

        // Phase 7.2: Validation Pipeline Performance
        Console.WriteLine("Running Phase 7.2: Validation Pipeline Benchmarks...");
        var validationSummary = BenchmarkRunner.Run<ValidationBenchmarks>();
        Console.WriteLine();

        // Phase 7.3: Hash Computation Performance
        Console.WriteLine("Running Phase 7.3: Hash Computation Benchmarks...");
        var hashSummary = BenchmarkRunner.Run<HashComputationBenchmarks>();
        Console.WriteLine();

        // Phase 7.4: Provenance Serialization Performance
        Console.WriteLine("Running Phase 7.4: Provenance Serialization Benchmarks...");
        var provenanceSummary = BenchmarkRunner.Run<ProvenanceBenchmarks>();
        Console.WriteLine();

        // Original: Template Generation Performance (baseline)
        Console.WriteLine("Running Baseline: Template Generation Benchmarks...");
        var generationSummary = BenchmarkRunner.Run<GenerationBenchmarks>();
        Console.WriteLine();

        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine("Phase 7 Benchmarking Complete");
        Console.WriteLine("=".PadRight(80, '='));
        Console.WriteLine();
        Console.WriteLine("Results saved to: BenchmarkDotNet.Artifacts/results/");
    }
}
