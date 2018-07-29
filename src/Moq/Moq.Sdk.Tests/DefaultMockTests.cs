using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Stunts;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class DefaultMockTests
    {
        [Fact]
        public void AddsMockTrackingBehavior()
        {
            var mock = new DefaultMock(new FakeStunt());

            Assert.Collection(mock.Behaviors, x => Assert.IsType<MockTrackingBehavior>(x));
        }

        [Fact]
        public void PreventsDuplicateMockTrackingBehavior()
        {
            var mock = new DefaultMock(new FakeStunt());

            Assert.Throws<InvalidOperationException>(() => mock.Behaviors.Add(new MockTrackingBehavior()));
        }

        class FakeStunt : IStunt
        {
            public ObservableCollection<IStuntBehavior> Behaviors { get; } = new ObservableCollection<IStuntBehavior>();
        }
    }
}
