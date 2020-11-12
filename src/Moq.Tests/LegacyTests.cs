using System;
using System.Threading.Tasks;
using Xunit;

namespace Moq.Tests
{
    public class LegacyTests
    {
        public interface IAsync
        {
            Task RunVoid();
            Task<T> Run<T>();
            ValueTask RunValueVoid();
            ValueTask<T> RunValue<T>();
        }

        [Fact]
        public async Task AsyncTests()
        {
            var mock = new Mock<IAsync>();
            mock.Setup(x => x.RunVoid()).ThrowsAsync(new ArgumentException());
            await Assert.ThrowsAsync<ArgumentException>(async () => await mock.Object.RunVoid());

            mock = new Mock<IAsync>();
            mock.Setup(x => x.RunVoid()).ThrowsAsync<ArgumentException>();
            await Assert.ThrowsAsync<ArgumentException>(async () => await mock.Object.RunVoid());

            mock = new Mock<IAsync>();
            mock.Setup(x => x.Run<bool>()).ThrowsAsync(new ArgumentException());
            await Assert.ThrowsAsync<ArgumentException>(async () => await mock.Object.Run<bool>());

            mock = new Mock<IAsync>();
            mock.Setup(x => x.Run<bool>()).ThrowsAsync<ArgumentException>();
            await Assert.ThrowsAsync<ArgumentException>(async () => await mock.Object.Run<bool>());

            mock = new Mock<IAsync>();
            mock.Setup(x => x.RunValueVoid()).ThrowsAsync(new ArgumentException());
            await Assert.ThrowsAsync<ArgumentException>(async () => await mock.Object.RunValueVoid());

            mock = new Mock<IAsync>();
            mock.Setup(x => x.RunValueVoid()).ThrowsAsync<ArgumentException>();
            await Assert.ThrowsAsync<ArgumentException>(async () => await mock.Object.RunValueVoid());

            mock = new Mock<IAsync>();
            mock.Setup(x => x.RunValue<bool>()).ThrowsAsync(new ArgumentException());
            await Assert.ThrowsAsync<ArgumentException>(async () => await mock.Object.RunValue<bool>());

            mock = new Mock<IAsync>();
            mock.Setup(x => x.RunValue<bool>()).ThrowsAsync<ArgumentException>();
            await Assert.ThrowsAsync<ArgumentException>(async () => await mock.Object.RunValue<bool>());
        }

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
