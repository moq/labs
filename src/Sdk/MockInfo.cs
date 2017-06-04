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
    /// Default implementation of <see cref="IMock"/> for inspecting 
    /// a mock.
    /// </summary>
    public class MockInfo : IMock
    {
        ConcurrentDictionary<IMockSetup, IMockBehavior> setupBehaviorMap = new ConcurrentDictionary<IMockSetup, IMockBehavior>();

        public MockInfo(ObservableCollection<IProxyBehavior> behaviors)
        {
            Behaviors = behaviors;
            behaviors.CollectionChanged += OnBehaviorsChanged;
        }

        /// <inheritdoc />
        public IList<IProxyBehavior> Behaviors { get; }

        /// <inheritdoc />
        public IList<IMethodInvocation> Invocations { get; } = new List<IMethodInvocation>();

        public IMockSetup LastSetup { get; internal set; }

        /// <inheritdoc />
        public MockState State { get; } = new MockState();

        public IMockBehavior BehaviorFor(IMockSetup setup)
            => setupBehaviorMap.GetOrAdd(setup, x => new MockBehavior(x));

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