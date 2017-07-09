using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Moq.Proxy;

namespace Moq.Sdk
{
    /// <summary>
    /// Default implementation of the mock introspection API <see cref="IMock"/>
    /// </summary>
    public class MockInfo : IMock
    {
        IProxy proxy;
        ConcurrentDictionary<IMockSetup, IMockBehavior> setupBehaviorMap = new ConcurrentDictionary<IMockSetup, IMockBehavior>();

        public MockInfo(IProxy proxy)
        {
            this.proxy = proxy ?? throw new ArgumentNullException(nameof(proxy));
            proxy.Behaviors.CollectionChanged += OnBehaviorsChanged;
        }

        /// <inheritdoc />
        public ObservableCollection<IProxyBehavior> Behaviors => proxy.Behaviors;

        /// <inheritdoc />
        public IList<IMethodInvocation> Invocations { get; } = new List<IMethodInvocation>();

        public IMockSetup LastSetup { get; internal set; }

        /// <inheritdoc />
        public MockState State { get; } = new MockState();

        public IMockBehavior BehaviorFor(IMockSetup setup)
            => setupBehaviorMap.GetOrAdd(setup, x =>
            {
                var behavior = new MockBehavior(x);
                Behaviors.Insert(1, behavior);
                return behavior;
            });

        void OnBehaviorsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    foreach (var behavior in e.NewItems.OfType<IMockBehavior>())
                        setupBehaviorMap.TryAdd(behavior.Setup, behavior);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var behavior in e.OldItems.OfType<IMockBehavior>())
                        setupBehaviorMap.TryRemove(behavior.Setup, out _);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var behavior in e.NewItems.OfType<IMockBehavior>())
                        setupBehaviorMap.TryAdd(behavior.Setup, behavior);
                    foreach (var behavior in e.OldItems.OfType<IMockBehavior>())
                        setupBehaviorMap.TryRemove(behavior.Setup, out _);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    setupBehaviorMap.Clear();
                    foreach (var behavior in Behaviors.OfType<IMockBehavior>())
                        setupBehaviorMap.TryAdd(behavior.Setup, behavior);
                    break;
                default:
                    break;
            }
        }
    }
}