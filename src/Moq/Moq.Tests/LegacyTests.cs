using System;
using Xunit;

namespace Moq.Tests
{
    public class LegacyTests
    {
        [Fact]
        public void AsInterface()
        {
            var sp = new Mock<IServiceProvider>();

            sp.Setup(x => x.GetService(typeof(IDisposable))).Returns(Mock.Of<IDisposable>());

            sp.As<IDisposable>()
                .Setup(x => x.Dispose())
                .Callback(() => Assert.False(true, "Callback"));

            Assert.NotNull(sp.Object.GetService(typeof(IDisposable)));

            var disposable = sp.Object as IDisposable;

            Assert.NotNull(disposable);

            disposable.Dispose();
        }
    }
}
