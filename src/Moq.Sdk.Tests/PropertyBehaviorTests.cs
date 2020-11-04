using System;
using System.Reflection;
using Avatars;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class PropertyBehaviorTests
    {
        [Fact]
        public void AppliesToGet()
        {
            var behavior = new PropertyBehavior();
            Assert.True(behavior.AppliesTo(new MethodInvocation(
                new PropertiesMock(),
                typeof(PropertiesMock).GetProperty(nameof(PropertiesMock.Id)).GetGetMethod())));
        }

        [Fact]
        public void AppliesToSet()
        {
            var behavior = new PropertyBehavior();
            Assert.True(behavior.AppliesTo(new MethodInvocation(
                new PropertiesMock(),
                typeof(PropertiesMock).GetProperty(nameof(PropertiesMock.Id)).GetSetMethod(),
                "foo")));
        }

        [Fact]
        public void ImplementsBackingState()
        {
            var mock = new PropertiesMock();
            mock.AddBehavior(new PropertyBehavior());

            mock.Id = "foo";

            Assert.Equal("foo", mock.Id);
        }

        [Fact]
        public void ThrowsIfNullInvocation()
            => Assert.Throws<ArgumentNullException>(() 
                => new PropertyBehavior().Execute(null, () => throw new NotImplementedException()));

        [Fact]
        public void ThrowsIfTargetNotMocked()
        {
            var behavior = new PropertyBehavior();
            Assert.Throws<ArgumentException>(() => behavior.Execute(new MethodInvocation(
                new object(),
                typeof(PropertiesMock).GetProperty(nameof(PropertiesMock.Id)).GetSetMethod()), 
                () => throw new NotImplementedException()));
        }

        public class PropertiesMock : FakeMock
        {
            public string Id
            {
                get => Pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
                set => Pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
            }
        }
    }
}
