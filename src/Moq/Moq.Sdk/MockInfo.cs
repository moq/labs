using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Linq;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Default implementation of the mock introspection API <see cref="IMock"/>
    /// </summary>
    public class MockInfo : IMock
    {
        IStunt stunt;
        ConcurrentDictionary<IMockSetup, IMockBehavior> setupBehaviorMap = new ConcurrentDictionary<IMockSetup, IMockBehavior>();

        public MockInfo(IStunt stunt)
        {
            this.stunt = stunt ?? throw new ArgumentNullException(nameof(stunt));
            stunt.Behaviors.CollectionChanged += OnBehaviorsChanged;
        }

        /// <inheritdoc />
        public ObservableCollection<IStuntBehavior> Behaviors => stunt.Behaviors;

        /// <inheritdoc />
        public IList<IMethodInvocation> Invocations { get; } = new List<IMethodInvocation>();

        public IMockSetup LastSetup { get; internal set; }

        /// <inheritdoc />
        public MockState State { get; } = new MockState();

        public IMockBehavior BehaviorFor(IMockSetup setup)
            => setupBehaviorMap.GetOrAdd(setup, x =>
            {
                var behavior = new MockBehavior(x);
                // The tracking behavior must appear before the mock behaviors.
                var tracking = Behaviors.OfType<MockTrackingBehavior>().FirstOrDefault();
                var index = tracking == null ? 0 : (Behaviors.IndexOf(tracking) + 1);
                Behaviors.Insert(index, behavior);
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