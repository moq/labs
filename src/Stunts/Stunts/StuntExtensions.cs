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
		public static IStunt AddBehavior(this IStunt stunt, InvokeBehavior behavior, AppliesTo appliesTo = null, string name = null)
        {
            stunt.Behaviors.Add(StuntBehavior.Create(behavior, appliesTo, name));
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
		public static TStunt AddBehavior<TStunt>(this TStunt stunt, InvokeBehavior behavior, AppliesTo appliesTo = null, string name = null)
        {
            // We can't just add a constraint to the method signature, because 
            // proxies are typically geneated and don't expose the IProxy interface directly.
            if (stunt is IStunt target)
                target.Behaviors.Add(StuntBehavior.Create(behavior, appliesTo, name));
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
		public static IStunt InsertBehavior(this IStunt stunt, int index, InvokeBehavior behavior, AppliesTo appliesTo = null, string name = null)
        {
            stunt.Behaviors.Insert(index, StuntBehavior.Create(behavior, appliesTo, name));
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
        /// Inserts a behavior into the stunt behasvior pipeline at the specified 
        /// index.
        /// </summary>
        public static TStunt InsertBehavior<TStunt>(this TStunt stunt, int index, InvokeBehavior behavior, AppliesTo appliesTo = null, string name = null)
        {
            if (stunt is IStunt target)
                target.Behaviors.Insert(index, StuntBehavior.Create(behavior, appliesTo, name));
            else
                throw new ArgumentException(nameof(stunt));

            return stunt;
        }

        /// <summary>
        /// Inserts a behavior into the stunt behasvior pipeline at the specified 
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
