using Sample;
using Xunit;
using Xunit.Abstractions;

namespace Stunts.Tests
{
    public class ManualStuntTest
    {
        ITestOutputHelper output; 

        public ManualStuntTest(ITestOutputHelper output) => this.output = output;

        [Fact(Skip = "Just an example of the capabilities of a manually coded stunt.")]
        public void CanUseManualStunt()
        {
            var stunt = new CalculatorClassStunt();
            var recorder = new RecordingBehavior();
            stunt.AddBehavior(recorder);
            
            var isOn = false;
            stunt.TurnedOn += (_, __) => isOn = true;

            stunt.TurnOn();

            Assert.True(isOn);
            Assert.Equal(3, stunt.Add(1, 2));
            Assert.Equal(default, stunt.Mode);

            stunt.Store("balance", 100);
            Assert.Equal(100, stunt.Recall("balance"));

            stunt.Store(null, 0);
            Assert.Equal(0, stunt.Recall(null));

            var x = 1;
            var y = 2;
            var z = 0;
            Assert.True(stunt.TryAdd(ref x, ref y, out z));
            Assert.Equal(3, z);

            output.WriteLine(recorder.ToString());
        }
    }
}