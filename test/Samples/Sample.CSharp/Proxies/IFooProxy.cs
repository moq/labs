using System;
using System.Collections.ObjectModel;
using System.Reflection;
using Moq.Proxy;
using System.Runtime.CompilerServices;
using System.Threading;
using Moq.Sdk;

namespace Proxies
{
    [CompilerGenerated]
    public partial class IFooProxy : IFoo, IProxy, IMocked
    {
        BehaviorPipeline pipeline = new BehaviorPipeline();

        ObservableCollection<IProxyBehavior> IProxy.Behaviors => pipeline.Behaviors;

        public string Id => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));

        public string Title { get => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }

        public override bool Equals(object obj) => pipeline.Execute<bool>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), obj));
        public override int GetHashCode() => pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
        public override string ToString() => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));

        #region IMocked
        IMock mock;

        IMock IMocked.Mock => LazyInitializer.EnsureInitialized(ref mock, () => new MockInfo(this));
        #endregion
    }
}