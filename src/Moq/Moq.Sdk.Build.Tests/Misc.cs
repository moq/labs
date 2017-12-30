using System.ComponentModel;
using System;
using System.Collections.Generic;
using System.Reflection;
using Moq.Proxy;
using System.Runtime.CompilerServices;

[CompilerGeneratedAttribute]
public partial class INotifyPropertyChangedINotifyPropertyChangingProxy : INotifyPropertyChanged, INotifyPropertyChanging, IProxy
{
    BehaviorPipeline pipeline = new BehaviorPipeline();
    public event PropertyChangedEventHandler PropertyChanged
    {
        add
        {
            pipeline.Execute<PropertyChangedEventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
            propertyChanged = ((PropertyChangedEventHandler)(Delegate.Combine(propertyChanged, value)));
        }

        remove
        {
            pipeline.Execute<PropertyChangedEventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
            propertyChanged = ((PropertyChangedEventHandler)(Delegate.Remove(propertyChanged, value)));
        }
    }

    public event PropertyChangingEventHandler PropertyChanging
    {
        add
        {
            pipeline.Execute<PropertyChangingEventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
            propertyChanging = ((PropertyChangingEventHandler)(Delegate.Combine(propertyChanging, value)));
        }

        remove
        {
            pipeline.Execute<PropertyChangingEventHandler>(new MethodInvocation(this, MethodBase.GetCurrentMethod(), value));
            propertyChanging = ((PropertyChangingEventHandler)(Delegate.Remove(propertyChanging, value)));
        }
    }

    IList<IProxyBehavior> IProxy.Behaviors => pipeline.Behaviors;
    PropertyChangedEventHandler propertyChanged;
    PropertyChangingEventHandler propertyChanging;

    delegate void ChotaHanlder(string name, int index);

    public Delegate GetEvent(string eventName)
    {
        switch (eventName)
        {
            case nameof(PropertyChanged):
                return propertyChanged;
            case nameof(PropertyChanging):
                return propertyChanging;
            default:
                return null;
        }
    }
}