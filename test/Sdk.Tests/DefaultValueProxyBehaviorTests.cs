using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Moq.Proxy;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class DefaultValueProxyBehaviorTests
    {
        [Fact]
        public void SetsRefValue()
        {
            var method = typeof(IDefaultValues).GetMethod(nameof(IDefaultValues.VoidWithRef));
            var behavior = new DefaultValueProxyBehavior();

            var result = behavior.Invoke(new MethodInvocation(new object(), method, new object[1]), () => null);

            Assert.Equal(1, result.Outputs.Count);
            Assert.NotNull(result.Outputs[0]);
            Assert.True(result.Outputs[0] is object[]);
        }
        
        [Fact]
        public void SetsRefEnumValue()
        {
            var method = typeof(IDefaultValues).GetMethod(nameof(IDefaultValues.VoidWithRefEnum));
            var behavior = new DefaultValueProxyBehavior();

            var result = behavior.Invoke(new MethodInvocation(new object(), method, new object[1]), () => null);

            Assert.Equal(1, result.Outputs.Count);
            Assert.NotNull(result.Outputs[0]);
            Assert.Equal(default(PlatformID), result.Outputs[0]);
        }

        [Fact]
        public void SetsOutValue()
        {
            var method = typeof(IDefaultValues).GetMethod(nameof(IDefaultValues.VoidWithOut));
            var behavior = new DefaultValueProxyBehavior();

            var result = behavior.Invoke(new MethodInvocation(new object(), method, new object[1]), () => null);

            Assert.Equal(1, result.Outputs.Count);
            Assert.NotNull(result.Outputs[0]);
            Assert.True(result.Outputs[0] is object[]);
        }

        [Fact]
        public void SetsReturnEnum()
        {
            var method = typeof(IDefaultValues).GetMethod(nameof(IDefaultValues.ReturnEnum));
            var behavior = new DefaultValueProxyBehavior();

            var result = behavior.Invoke(new MethodInvocation(new object(), method, new object[0]), () => null);

            Assert.Equal(default(PlatformID), result.ReturnValue);
        }

        [Fact]
        public void SetsReturnNullableEnum()
        {
            var method = typeof(IDefaultValues).GetMethod(nameof(IDefaultValues.ReturnNullableEnum));
            var behavior = new DefaultValueProxyBehavior();

            var result = behavior.Invoke(new MethodInvocation(new object(), method, new object[0]), () => null);

            Assert.Null(result.ReturnValue);
        }


        [Fact]
        public void SetsReturnArray()
        {
            var method = typeof(IDefaultValues).GetMethod(nameof(IDefaultValues.ReturnArray));
            var behavior = new DefaultValueProxyBehavior();

            var result = behavior.Invoke(new MethodInvocation(new object(), method, new object[0]), () => null);

            Assert.NotNull(result.ReturnValue);
            Assert.True(result.ReturnValue is object[]);
            Assert.Equal(0, ((Array)result.ReturnValue).Length);
        }

        [Fact]
        public void SetsReturnTask()
        {
            var method = typeof(IDefaultValues).GetMethod(nameof(IDefaultValues.ReturnTask));
            var behavior = new DefaultValueProxyBehavior();

            var result = behavior.Invoke(new MethodInvocation(new object(), method, new object[0]), () => null);

            Assert.NotNull(result.ReturnValue);
            Assert.True(result.ReturnValue is Task);
            Assert.True(((Task)result.ReturnValue).IsCompleted);
        }

        [Fact]
        public void SetsReturnGenericTask()
        {
            var method = typeof(IDefaultValues).GetMethod(nameof(IDefaultValues.ReturnGenericTask));
            var behavior = new DefaultValueProxyBehavior();

            var result = behavior.Invoke(new MethodInvocation(new object(), method, new object[0]), () => null);

            Assert.NotNull(result.ReturnValue);
            Assert.True(result.ReturnValue is Task<object>);
            Assert.True(((Task)result.ReturnValue).IsCompleted);
        }

        [Fact]
        public void SetsReturnGenericTaskEnum()
        {
            var method = typeof(IDefaultValues).GetMethod(nameof(IDefaultValues.ReturnGenericTaskEnum));
            var behavior = new DefaultValueProxyBehavior();

            var result = behavior.Invoke(new MethodInvocation(new object(), method, new object[0]), () => null);

            Assert.NotNull(result.ReturnValue);
            Assert.True(result.ReturnValue is Task<PlatformID>);
            Assert.True(((Task)result.ReturnValue).IsCompleted);
            Assert.Equal(default(PlatformID), ((Task<PlatformID>)result.ReturnValue).Result);
        }

        [Fact]
        public void SetsReturnEnumerable()
        {
            var method = typeof(IDefaultValues).GetMethod(nameof(IDefaultValues.ReturnEnumerable));
            var behavior = new DefaultValueProxyBehavior();

            var result = behavior.Invoke(new MethodInvocation(new object(), method, new object[0]), () => null);

            Assert.NotNull(result.ReturnValue);
            Assert.True(result.ReturnValue is IEnumerable);
        }

        [Fact]
        public void SetsReturnGenericEnumerable()
        {
            var method = typeof(IDefaultValues).GetMethod(nameof(IDefaultValues.ReturnGenericEnumerable));
            var behavior = new DefaultValueProxyBehavior();

            var result = behavior.Invoke(new MethodInvocation(new object(), method, new object[0]), () => null);

            Assert.NotNull(result.ReturnValue);
            Assert.True(result.ReturnValue is IEnumerable<object>);
        }

        [Fact]
        public void DefaultValueForRef()
        {
            var method = typeof(IDefaultValues).GetMethod(nameof(IDefaultValues.VoidWithRef));
            var parameter = method.GetParameters()[0];

            Assert.True(parameter.ParameterType.IsByRef);

            var value = DefaultValue.For(parameter.ParameterType);

            Assert.NotNull(value);
            Assert.True(value is object[]);
            Assert.Empty((object[])value);
        }

        public interface IDefaultValues
        {
            void VoidWithRef(ref object[] refValue);

            void VoidWithRefEnum(ref PlatformID refEnum);

            void VoidWithOut(out object[] refValue);

            PlatformID ReturnEnum();

            Nullable<PlatformID> ReturnNullableEnum();

            object[] ReturnArray();

            Task ReturnTask();

            Task<object> ReturnGenericTask();

            Task<PlatformID> ReturnGenericTaskEnum();

            IEnumerable ReturnEnumerable();

            IEnumerable<object> ReturnGenericEnumerable();
        }
    }
}
