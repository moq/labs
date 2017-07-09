using System.ComponentModel;
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
    public partial class INotifyPropertyChangedProxy : INotifyPropertyChanged, IProxy, IMocked
    {
        BehaviorPipeline pipeline = new BehaviorPipeline();

        ObservableCollection<IProxyBehavior> IProxy.Behaviors => pipeline.Behaviors;

        public event PropertyChangedEventHandler PropertyChanged { add => pipeline.Execute<PropertyChangedEventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); remove => pipeline.Execute<PropertyChangedEventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value)); }

        #region IMocked
        IMock mock;

        IMock IMocked.Mock => LazyInitializer.EnsureInitialized(ref mock, () => new MockInfo(this));
        #endregion
    }
}