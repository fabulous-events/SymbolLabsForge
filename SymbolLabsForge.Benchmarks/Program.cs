using BenchmarkDotNet.Running;
using SymbolLabsForge.Benchmarks.Benchmarks;

public class Program
{
    public static void Main(string[] args)
    {
        var summary = BenchmarkRunner.Run<GenerationBenchmarks>();
    }
}
