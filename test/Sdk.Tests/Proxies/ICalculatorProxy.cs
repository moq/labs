using Moq.Sdk.Tests;
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
    public partial class ICalculatorProxy : ICalculator, IProxy, IMocked
    {
        BehaviorPipeline pipeline = new BehaviorPipeline();
        ObservableCollection<IProxyBehavior> IProxy.Behaviors => pipeline.Behaviors;

        public string Mode
        {
            get => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
            set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
        }

        public int Add(int a, int b) => pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), a, b));
        public void PowerUp() => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
        public event EventHandler PoweringUp
        {
            add => pipeline.Execute<EventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
            remove => pipeline.Execute<EventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
        }

#region IMocked
        IMock mock;
        IMock IMocked.Mock => LazyInitializer.EnsureInitialized(ref mock, () => new MockInfo(this));
#endregion
    }
}