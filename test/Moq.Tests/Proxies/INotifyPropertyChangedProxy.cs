using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using Moq.Proxy;
using System.Runtime.CompilerServices;
using System.Threading;
using Moq.Sdk;

namespace Proxies
{
    [CompilerGenerated]
    public partial class INotifyPropertyChangedProxy : INotifyPropertyChanged, IProxy, IMocked
    {
        public event PropertyChangedEventHandler PropertyChanged { add => pipeline.Execute<PropertyChangedEventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); remove => pipeline.Execute<PropertyChangedEventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }

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