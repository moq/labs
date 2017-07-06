using System;
using System.Collections.Generic;
using System.Reflection;
using Moq.Proxy;
using System.Runtime.CompilerServices;
using System.ComponentModel;
using System.Threading;
using Moq.Sdk;

namespace Proxies
{
    [CompilerGenerated]
    public partial class ICalculatorProxy : ICalculator, IProxy, IMocked
    {
        public string Mode
        {
            get => pipeline.Execute<string>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
            set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
        }

        public int Add(int a, int b) => pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), a, b));
        public void PowerUp() => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod()));
        public bool TryAdd(ref int x, ref int y, out int z)
        {
            z = default (int);
            IMethodReturn returns = pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), x, y, z));
            x = (int)returns.Outputs["x"];
            y = (int)returns.Outputs["y"];
            z = (int)returns.Outputs["z"];
            return (bool)returns.ReturnValue;
        }

        public event PropertyChangedEventHandler PropertyChanged
        {
            add => pipeline.Execute<PropertyChangedEventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
            remove => pipeline.Execute<PropertyChangedEventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
        }

        public event EventHandler<int> Progress
        {
            add => pipeline.Execute<EventHandler<int>>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
            remove => pipeline.Execute<EventHandler<int>>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
        }

        public event EventHandler PoweringUp
        {
            add => pipeline.Execute<EventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
            remove => pipeline.Execute<EventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
        }

#region IProxy
        BehaviorPipeline pipeline = new BehaviorPipeline();
        IList<IProxyBehavior> IProxy.Behaviors => pipeline.Behaviors;
#endregion
#region IMocked
        IMock mock;
        IMock IMocked.Mock => LazyInitializer.EnsureInitialized(ref mock, () => new MockInfo(pipeline.Behaviors));
#endregion
    }
}