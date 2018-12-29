using System;
using System.ComponentModel;
using System.Linq;
using Moq.Properties;
using Moq.Sdk;
using Stunts;

namespace Moq
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class MoqExtensions
    {
        /// <summary>
        /// Clears any existing behavior (including any setups) and 
        /// adds the necessary behaviors to the <paramref name="mocked"/> so 
        /// that it behaves as specified by the <paramref name="behavior"/> 
        /// enumeration.
        /// </summary>
        /// <remarks>
        /// This method can be used by custom mocks to ensure they have the 
        /// same default behaviors as a mock created using <c>Mock.Of{T}</c>.
        /// </remarks>
        public static void Initialize(this IMocked mocked, MockBehavior behavior)
        {
            mocked.Mock.Behaviors.Clear();

            mocked.Mock.Behaviors.Add(new MockTrackingBehavior());
            mocked.Mock.Behaviors.Add(new EventBehavior());
            mocked.Mock.Behaviors.Add(new PropertyBehavior { SetterRequiresSetup = behavior == MockBehavior.Strict });
            mocked.Mock.Behaviors.Add(new DefaultEqualityBehavior());
            mocked.Mock.Behaviors.Add(new RecursiveMockBehavior());

            if (behavior == MockBehavior.Strict)
            {
                mocked.Mock.Behaviors.Add(new StrictMockBehavior());
            }
            else
            {
                var defaultValue = mocked.Mock.State.GetOrAdd(() => new DefaultValueProvider());
                mocked.Mock.Behaviors.Add(new DefaultValueBehavior(defaultValue));
                mocked.Mock.State.Set(defaultValue);
            }

            mocked.Mock.State.Set(behavior);
            // Saves the initial set of behaviors, which allows resetting the mock.
            mocked.Mock.State.Set(mocked.Mock.Behaviors.ToArray());
        }

        /// <summary>
        /// Gets the mock configuration corresponding for the given instance.
        /// </summary>
        public static IMoq<T> AsMoq<T>(this T instance) => new Moq<T>(instance.AsMock());

        class Moq<T> : IMoq<T>
        {
            public Moq(IMock<T> mock) => Sdk = mock;

            public IMock<T> Sdk { get; private set; }

            // NOTE: the setter is somewhat duplicating behavior in Initialize...
            public MockBehavior Behavior
            {
                get => Sdk.Behaviors.Any(x => x is StrictMockBehavior) ? MockBehavior.Strict :
                    Sdk.Behaviors.Any(x => x is DefaultValueBehavior) ? MockBehavior.Loose :
                    throw new NotSupportedException(Strings.TargetNotLooseOrStrict(nameof(StrictMockBehavior), nameof(DefaultValueBehavior)));
                set
                {
                    if (value == MockBehavior.Loose)
                    {
                        var defaultValue = Sdk.State.GetOrAdd(() => new DefaultValueProvider());
                        var strict = Sdk.Behaviors.FirstOrDefault(x => x is StrictMockBehavior);
                        if (strict != null)
                        {
                            var index = Sdk.Behaviors.IndexOf(strict);
                            Sdk.Behaviors.Remove(strict);
                            Sdk.Behaviors.Insert(index, new DefaultValueBehavior(defaultValue));
                        }
                        else if (!Sdk.Behaviors.Any(x => x is DefaultValueBehavior))
                        {
                            Sdk.Behaviors.Add(new DefaultValueBehavior(defaultValue));
                        }

                        var propertyBehavior = Sdk.Behaviors.OfType<PropertyBehavior>().FirstOrDefault();
                        if (propertyBehavior != null)
                            propertyBehavior.SetterRequiresSetup = false;
                    }
                    else if (value == MockBehavior.Strict)
                    {
                        var loose = Sdk.Behaviors.FirstOrDefault(x => x is DefaultValueBehavior);
                        if (loose != null)
                        {
                            var index = Sdk.Behaviors.IndexOf(loose);
                            Sdk.Behaviors.Remove(loose);
                            Sdk.Behaviors.Insert(index, new StrictMockBehavior());
                        }
                        else if (!Sdk.Behaviors.Any(x => x is StrictMockBehavior))
                        {
                            Sdk.Behaviors.Add(new StrictMockBehavior());
                        }

                        var propertyBehavior = Sdk.Behaviors.OfType<PropertyBehavior>().FirstOrDefault();
                        if (propertyBehavior != null)
                            propertyBehavior.SetterRequiresSetup = true;
                    }
                }
            }

            public DefaultValueProvider DefaultValue
            {
                get => Sdk.State.GetOrAdd(() => new DefaultValueProvider());
                set
                {
                    if (value == null)
                        throw new ArgumentNullException(nameof(value));

                    if (Sdk.State.TryGetValue<DefaultValueProvider>(out var defaultValue) && 
                        value != defaultValue && 
                        Sdk.Behaviors.OfType<DefaultValueBehavior>().FirstOrDefault() is DefaultValueBehavior behavior && 
                        behavior != null)
                    {
                        behavior.Provider = value;
                    }

                    Sdk.State.Set(value);
                }
            }

            IMock IMoq.Sdk => Sdk;
        }
    }
}
