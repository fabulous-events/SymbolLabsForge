using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;
using SymbolLabsForge.Generators;
using SixLabors.ImageSharp;

namespace SymbolLabsForge.Benchmarks.Benchmarks
{
    [SimpleJob(RuntimeMoniker.Net80)]
    [MemoryDiagnoser]
    public class GenerationBenchmarks
    {
        private readonly FlatGenerator _generator = new FlatGenerator();
        private readonly Size _dimensions = new Size(12, 30);

        [Benchmark(Description = "FlatGenerator.GenerateRawImage")]
        public void BenchmarkFlatGenerator()
        {
            using var image = _generator.GenerateRawImage(_dimensions, null);
        }
    }
}
