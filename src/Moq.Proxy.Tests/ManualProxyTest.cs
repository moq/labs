using Xunit;
using Xunit.Abstractions;

namespace Moq.Proxy.Tests
{
    public class ManualProxyTest
    {
        ITestOutputHelper output; 

        public ManualProxyTest(ITestOutputHelper output) => this.output = output;

        [Fact]
        public void CanUseManualProxy()
        {
            var proxy = new CalculatorClassProxy();
            var recorder = new RecordingBehavior();
            proxy.AddProxyBehavior(recorder);
            
            var isOn = false;
            proxy.TurnedOn += (_, __) => isOn = true;

            proxy.TurnOn();

            Assert.True(isOn);
            Assert.Equal(3, proxy.Add(1, 2));
            Assert.Equal(default(CalculatorMode), proxy.Mode);

            proxy.Store("balance", 100);
            Assert.Equal(100, proxy.Recall("balance"));

            proxy.Store(null, 0);
            Assert.Equal(0, proxy.Recall(null));

            var x = 1;
            var y = 2;
            var z = 0;
            Assert.True(proxy.TryAdd(ref x, ref y, out z));
            Assert.Equal(3, z);

            output.WriteLine(recorder.ToString());
        }
    }
}