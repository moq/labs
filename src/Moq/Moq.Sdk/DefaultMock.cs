using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Linq;
using Moq.Sdk.Properties;
using Stunts;

namespace Moq.Sdk
{
    /// <summary>
    /// Default implementation of the mock introspection API <see cref="IMock"/>, 
    /// which also ensures that the <see cref="IStunt.Behaviors"/> contains 
    /// the <see cref="MockContextBehavior"/> when initially created.
    /// </summary>
    [DebuggerDisplay("Invocations = {Invocations.Count}", Name = nameof(IMocked) + "." + nameof(IMocked.Mock))]
    public class DefaultMock : IMock
    {
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly IStunt stunt;
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        private readonly ConcurrentDictionary<IMockSetup, IMockBehaviorPipeline> setupBehaviorMap = new ConcurrentDictionary<IMockSetup, IMockBehaviorPipeline>();

        /// <summary>
        /// Initializes the default <see cref="IMock"/> implementation for the given <paramref name="stunt"/>.
        /// </summary>
        public DefaultMock(IStunt stunt)
        {
            this.stunt = stunt ?? throw new ArgumentNullException(nameof(stunt));

            if (!stunt.Behaviors.OfType<MockContextBehavior>().Any())
                stunt.Behaviors.Insert(0, new MockContextBehavior());

            if (!stunt.Behaviors.OfType<MockRecordingBehavior>().Any())
                stunt.Behaviors.Insert(1, new MockRecordingBehavior());

            stunt.Behaviors.CollectionChanged += OnBehaviorsChanged;
        }

        /// <inheritdoc />
        public ObservableCollection<IStuntBehavior> Behaviors => stunt.Behaviors;

        /// <inheritdoc />
        public ICollection<IMethodInvocation> Invocations { get; } = new List<IMethodInvocation>();

        /// <inheritdoc />
        [DebuggerBrowsable(DebuggerBrowsableState.Never)]
        public object Object => stunt;

        /// <inheritdoc />
        public StateBag State { get; set; } = new StateBag();

        /// <inheritdoc />
        public IEnumerable<IMockBehaviorPipeline> Setups => setupBehaviorMap.Values;

        /// <inheritdoc />
        public IMockBehaviorPipeline GetPipeline(IMockSetup setup)
            => setupBehaviorMap.GetOrAdd(setup, x =>
            {
                var behavior = new MockBehaviorPipeline(x);
                // The tracking behavior must appear before the mock behaviors.
                var context = Behaviors.OfType<MockContextBehavior>().FirstOrDefault();
                // If there is a recording behavior, it must be before mock behaviors too.
                var recording = Behaviors.OfType<MockRecordingBehavior>().FirstOrDefault();

                var index = context == null ? 0 : Behaviors.IndexOf(context);
                if (recording != null)
                    index = Math.Max(index, Behaviors.IndexOf(recording));

                // NOTE: latest setup wins, since it goes to the top of the list.
                Behaviors.Insert(++index, behavior);
                return behavior;
            });

        void OnBehaviorsChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            switch (e.Action)
            {
                case NotifyCollectionChangedAction.Add:
                    // Can't have more than one of MockContextBehavior, since that causes problems.
                    if (Behaviors.OfType<MockContextBehavior>().Skip(1).Any())
                        throw new InvalidOperationException(Resources.DuplicateContextBehavior);
                    // Can't have more than one of MockRecordingBehavior, since that causes problems.
                    if (Behaviors.OfType<MockRecordingBehavior>().Skip(1).Any())
                        throw new InvalidOperationException(Resources.DuplicateRecordingBehavior);

                    foreach (var behavior in e.NewItems.OfType<IMockBehaviorPipeline>())
                        setupBehaviorMap.TryAdd(behavior.Setup, behavior);
                    break;
                case NotifyCollectionChangedAction.Remove:
                    foreach (var behavior in e.OldItems.OfType<IMockBehaviorPipeline>())
                        setupBehaviorMap.TryRemove(behavior.Setup, out _);
                    break;
                case NotifyCollectionChangedAction.Replace:
                    foreach (var behavior in e.NewItems.OfType<IMockBehaviorPipeline>())
                        setupBehaviorMap.TryAdd(behavior.Setup, behavior);
                    foreach (var behavior in e.OldItems.OfType<IMockBehaviorPipeline>())
                        setupBehaviorMap.TryRemove(behavior.Setup, out _);
                    break;
                case NotifyCollectionChangedAction.Reset:
                    setupBehaviorMap.Clear();
                    break;
            }
        }
    }
}