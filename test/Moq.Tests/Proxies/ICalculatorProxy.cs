using Moq.Proxy;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Threading;
using Moq.Sdk;

namespace Proxies
{
    [CompilerGenerated]
    public partial class ICalculatorProxy : ICalculator, IProxy, IMocked
    {
        public int? this[string name] { get => pipeline.Execute<int?>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), name)); set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), name, value)); }

        public bool IsOn => pipeline.Execute<bool>(new MethodInvocation(this, MethodBase.GetCurrentMethod()));

        public CalculatorMode Mode { get => pipeline.Execute<CalculatorMode>(new MethodInvocation(this, MethodBase.GetCurrentMethod())); set => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }

        public int Add(int x, int y) => pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), x, y));
        public int Add(int x, int y, int z) => pipeline.Execute<int>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), x, y, z));
        public void Clear(string name) => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), name));
        public int? Recall(string name) => pipeline.Execute<int?>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), name));
        public void Store(string name, int value) => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), name, value));

        public bool TryAdd(ref int x, ref int y, out int z)
        {
            z = default(int);
            IMethodReturn returns = pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod(), x, y, z));
            x = (int)returns.Outputs["x"];
            y = (int)returns.Outputs["y"];
            z = (int)returns.Outputs["z"];
            return (bool)returns.ReturnValue;
        }

        public void TurnOn() => pipeline.Execute(new MethodInvocation(this, MethodBase.GetCurrentMethod()));

        public event EventHandler TurnedOn { add => pipeline.Execute<EventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); remove => pipeline.Execute<EventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }

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