using System;

namespace Stunts
{
    /// <summary>
    /// Usability functions for working with stunts.
    /// </summary>
	public static class StuntExtensions
    {
        /// <summary>
        /// Adds a behavior to a stunt.
        /// </summary>
        /// <param name="stunt">The stunt to add the behavior to.</param>
        /// <param name="behavior"><c>(invocation, next) => ...</c>.    Implements a behavior based on the received invocation.
        /// Optionally calls the next behavior with: <c>next().Invoke(invocation, next)</c>.
        /// </param>
        /// <param name="appliesTo"><c>invocation => [bool]</c>. Optionally applies a condition to the execution based on the received invocation.</param>
        /// <param name="name">Optionally names the behavior for easier troubleshooting when debugging.</param>
		public static IStunt AddBehavior(this IStunt stunt, ExecuteDelegate behavior, AppliesToDelegate appliesTo = null, string name = null)
        {
            stunt.Behaviors.Add(new DelegateStuntBehavior(behavior, appliesTo, name));
            return stunt;
        }

        /// <summary>
        /// Adds a behavior to a stunt.
        /// </summary>
		public static IStunt AddBehavior(this IStunt stunt, IStuntBehavior behavior)
        {
            stunt.Behaviors.Add(behavior);
            return stunt;
        }

        /// <summary>
        /// Adds a behavior to a stunt.
        /// </summary>
        /// <param name="stunt">The stunt to add the behavior to.</param>
        /// <param name="behavior"><c>(invocation, next) => ...</c>.    Implements a behavior based on the received invocation.
        /// Optionally calls the next behavior with: <c>next().Invoke(invocation, next)</c>.
        /// </param>
        /// <param name="appliesTo"><c>invocation => [bool]</c>. Optionally applies a condition to the execution based on the received invocation.</param>
        /// <param name="name">Optionally names the behavior for easier troubleshooting when debugging.</param>
		public static TStunt AddBehavior<TStunt>(this TStunt stunt, ExecuteDelegate behavior, AppliesToDelegate appliesTo = null, string name = null)
        {
            // We can't just add a constraint to the method signature, because 
            // proxies are typically generated and don't expose the IProxy interface directly.
            if (stunt is IStunt target)
                target.Behaviors.Add(new DelegateStuntBehavior(behavior, appliesTo, name));
            else
                throw new ArgumentException(nameof(stunt));

            return stunt;
        }

        /// <summary>
        /// Adds a behavior to a stunt.
        /// </summary>
		public static TStunt AddBehavior<TStunt>(this TStunt stunt, IStuntBehavior behavior)
        {
            if (stunt is IStunt target)
                target.Behaviors.Add(behavior);
            else
                throw new ArgumentException(nameof(stunt));

            return stunt;
        }

        /// <summary>
        /// Inserts a behavior into the stunt behavior pipeline at the specified 
        /// index.
        /// </summary>
        /// <param name="stunt">The stunt to insert the behavior to.</param>
        /// <param name="index">The index to insert the behavior at.</param>
        /// <param name="behavior"><c>(invocation, next) => ...</c>.    Implements a behavior based on the received invocation.
        /// Optionally calls the next behavior with: <c>next().Invoke(invocation, next)</c>.
        /// </param>
        /// <param name="appliesTo"><c>invocation => [bool]</c>. Optionally applies a condition to the execution based on the received invocation.</param>
        /// <param name="name">Optionally names the behavior for easier troubleshooting when debugging.</param>
		public static IStunt InsertBehavior(this IStunt stunt, int index, ExecuteDelegate behavior, AppliesToDelegate appliesTo = null, string name = null)
        {
            stunt.Behaviors.Insert(index, new DelegateStuntBehavior(behavior, appliesTo, name));
            return stunt;
        }

        /// <summary>
        /// Inserts a behavior into the stunt behavior pipeline at the specified 
        /// index.
        /// </summary>
        public static IStunt InsertBehavior(this IStunt stunt, int index, IStuntBehavior behavior)
        {
            stunt.Behaviors.Insert(index, behavior);
            return stunt;
        }

        /// <summary>
        /// Inserts a behavior into the stunt behavior pipeline at the specified
        /// index.
        /// </summary>
        /// <param name="stunt">The stunt to insert the behavior to.</param>
        /// <param name="index">The index to insert the behavior at.</param>
        /// <param name="behavior"><c>(invocation, next) => ...</c>.    Implements a behavior based on the received invocation.
        /// Optionally calls the next behavior with: <c>next().Invoke(invocation, next)</c>.
        /// </param>
        /// <param name="appliesTo"><c>invocation => [bool]</c>. Optionally applies a condition to the execution based on the received invocation.</param>
        /// <param name="name">Optionally names the behavior for easier troubleshooting when debugging.</param>
        public static TStunt InsertBehavior<TStunt>(this TStunt stunt, int index, ExecuteDelegate behavior, AppliesToDelegate appliesTo = null, string name = null)
        {
            if (stunt is IStunt target)
                target.Behaviors.Insert(index, new DelegateStuntBehavior(behavior, appliesTo, name));
            else
                throw new ArgumentException(nameof(stunt));

            return stunt;
        }

        /// <summary>
        /// Inserts a behavior into the stunt behavior pipeline at the specified
        /// index.
        /// </summary>
        public static TStunt InsertBehavior<TStunt>(this TStunt stunt, int index, IStuntBehavior behavior)
        {
            if (stunt is IStunt target)
                target.Behaviors.Insert(index, behavior);
            else
                throw new ArgumentException(nameof(stunt));

            return stunt;
        }
    }
}
