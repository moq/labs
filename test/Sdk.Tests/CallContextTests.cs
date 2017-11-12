using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit;

namespace Moq.Sdk.Tests
{
    public class CallContextTests
    {
        [Fact]
        public void WhenFlowingData_ThenCanUseContext()
        {
            var key1 = Guid.NewGuid().ToString();
            var d1 = new object();
            var t1 = default(object);
            var t10 = default(object);
            var t11 = default(object);
            var t12 = default(object);
            var t13 = default(object);

            var key2 = Guid.NewGuid().ToString();
            var d2 = new object();
            var t2 = default(object);
            var t20 = default(object);
            var t21 = default(object);
            var t22 = default(object);
            var t23 = default(object);

            Task.WaitAll(
                Task.Run(() =>
                {
                    CallContext.SetData(key1, d1);
                    new Thread(() => t10 = CallContext.GetData(key1)).Start();
                    Task.WaitAll(
                        Task.Run(() => t1 = CallContext.GetData(key1))
                            .ContinueWith(t => t11 = CallContext.GetData(key1)),
                        Task.Run(() => t12 = CallContext.GetData(key1)),
                        Task.Run(() => t13 = CallContext.GetData(key1))
                    );
                }),
                Task.Run(() =>
                {
                    CallContext.SetData(key2, d2);
                    new Thread(() => t20 = CallContext.GetData(key2)).Start();
                    Task.WaitAll(
                        Task.Run(() => t2 = CallContext.GetData(key2))
                            .ContinueWith(t => t21 = CallContext.GetData(key2)),
                        Task.Run(() => t22 = CallContext.GetData(key2)),
                        Task.Run(() => t23 = CallContext.GetData(key2))
                    );
                })
            );

            Assert.Same(d1, t1);
            Assert.Same(d1, t10);
            Assert.Same(d1, t11);
            Assert.Same(d1, t12);
            Assert.Same(d1, t13);

            Assert.Same(d2, t2);
            Assert.Same(d2, t20);
            Assert.Same(d2, t21);
            Assert.Same(d2, t22);
            Assert.Same(d2, t23);

            Assert.Null(CallContext.GetData(key1));
            Assert.Null(CallContext.GetData(key2));
        }

        [Fact]
        public void WhenFlowingData_ThenCanUseGenericContext()
        {
            var key1 = Guid.NewGuid().ToString();
            var d1 = new Foo();
            var t1 = default(Foo);
            var t10 = default(Foo);
            var t11 = default(Foo);
            var t12 = default(Foo);
            var t13 = default(Foo);

            var key2 = Guid.NewGuid().ToString();
            var d2 = new Foo();
            var t2 = default(Foo);
            var t20 = default(Foo);
            var t21 = default(Foo);
            var t22 = default(Foo);
            var t23 = default(Foo);

            Task.WaitAll(
                Task.Run(() =>
                {
                    CallContext<Foo>.SetData(key1, d1);
                    new Thread(() => t10 = CallContext<Foo>.GetData(key1)).Start();
                    Task.WaitAll(
                        Task.Run(() => t1 = CallContext<Foo>.GetData(key1))
                            .ContinueWith(t => t11 = CallContext<Foo>.GetData(key1)),
                        Task.Run(() => t12 = CallContext<Foo>.GetData(key1)),
                        Task.Run(() => t13 = CallContext<Foo>.GetData(key1))
                    );
                }),
                Task.Run(() =>
                {
                    CallContext<Foo>.SetData(key2, d2);
                    new Thread(() => t20 = CallContext<Foo>.GetData(key2)).Start();
                    Task.WaitAll(
                        Task.Run(() => t2 = CallContext<Foo>.GetData(key2))
                            .ContinueWith(t => t21 = CallContext<Foo>.GetData(key2)),
                        Task.Run(() => t22 = CallContext<Foo>.GetData(key2)),
                        Task.Run(() => t23 = CallContext<Foo>.GetData(key2))
                    );
                })
            );

            Assert.Same(d1, t1);
            Assert.Same(d1, t10);
            Assert.Same(d1, t11);
            Assert.Same(d1, t12);
            Assert.Same(d1, t13);

            Assert.Same(d2, t2);
            Assert.Same(d2, t20);
            Assert.Same(d2, t21);
            Assert.Same(d2, t22);
            Assert.Same(d2, t23);

            Assert.Null(CallContext<Foo>.GetData(key1));
            Assert.Null(CallContext<Foo>.GetData(key2));
        }

        public class Foo { }
    }
}
