using Microsoft.Extensions.Options;

namespace SymbolLabsForge.Tests.Validation
{
    public class MockOptions<T> : IOptions<T> where T : class, new()
    {
        public MockOptions(T value)
        {
            Value = value;
        }

        public T Value { get; }
    }
}
