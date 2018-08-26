using System.Threading.Tasks;
using Shouldly;
using Xunit;

namespace LightningBug.Polly.Wrapper
{
    public class BuildDelegateTests
    {

        public string Echo(string message)
        {
            return message;
        }

        public Task<string> EchoAsync(string message)
        {
            return Task.FromResult(message);
        }

        [Fact]
        public void CanCompileSyncMethod()
        {
            var mi = typeof(BuildDelegateTests).GetMethod(nameof(Echo));

            var func = mi.BuildDelegate<BuildDelegateTests>();

            const string expected = "This is my message!";
            var result = func(this, new object[] { expected });
            result.ShouldBe(expected);
        }

        [Fact]
        public async Task CanCompileAsyncMethod()
        {
            var mi = typeof(BuildDelegateTests).GetMethod(nameof(EchoAsync));

            var func = mi.BuildAsyncDelegate<BuildDelegateTests>();

            const string expected = "This is my message!";
            var result = await func(this, new object[] { expected });
            result.ShouldBe(expected);
        }


    }
}