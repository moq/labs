using System;
using System.ComponentModel;
using System.Linq;
using Moq.Properties;
using Moq.Sdk;

namespace Moq
{
    /// <summary>
    /// Provides the Moq specific configurations on top of an <see cref="Sdk.IMock{T}"/> that 
    /// the Moq API provides beyond the SDK.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class Moq<T> : MockDecorator<T>, IMoq<T>
    {
        /// <summary>
        /// Decorates the given <see cref="IMock{T}"/> with Moq specific 
        /// properties.
        /// </summary>
        public Moq(IMock<T> mock) : base(mock) { }

        /// <summary>
        /// Gets or sets the <see cref="MockBehavior"/> for the 
        /// mock.
        /// </summary>
        // NOTE: the setter is somewhat duplicating behavior in Initialize...
        public MockBehavior Behavior
        {
            get => Behaviors.Any(x => x is StrictMockBehavior) ? MockBehavior.Strict :
                Behaviors.Any(x => x is DefaultValueBehavior) ? MockBehavior.Loose :
                throw new NotSupportedException(Strings.TargetNotLooseOrStrict(nameof(StrictMockBehavior), nameof(DefaultValueBehavior)));
            set
            {
                if (value == MockBehavior.Loose)
                {
                    var defaultValue = State.GetOrAdd(() => new DefaultValueProvider());
                    var strict = Behaviors.FirstOrDefault(x => x is StrictMockBehavior);
                    if (strict != null)
                    {
                        var index = Behaviors.IndexOf(strict);
                        Behaviors.Remove(strict);
                        Behaviors.Insert(index, new DefaultValueBehavior(defaultValue));
                    }
                    else if (!Behaviors.Any(x => x is DefaultValueBehavior))
                    {
                        Behaviors.Add(new DefaultValueBehavior(defaultValue));
                    }

                    var propertyBehavior = Behaviors.OfType<PropertyBehavior>().FirstOrDefault();
                    if (propertyBehavior != null)
                        propertyBehavior.SetterRequiresSetup = false;
                }
                else if (value == MockBehavior.Strict)
                {
                    var loose = Behaviors.FirstOrDefault(x => x is DefaultValueBehavior);
                    if (loose != null)
                    {
                        var index = Behaviors.IndexOf(loose);
                        Behaviors.Remove(loose);
                        Behaviors.Insert(index, new StrictMockBehavior());
                    }
                    else if (!Behaviors.Any(x => x is StrictMockBehavior))
                    {
                        Behaviors.Add(new StrictMockBehavior());
                    }

                    var propertyBehavior = Behaviors.OfType<PropertyBehavior>().FirstOrDefault();
                    if (propertyBehavior != null)
                        propertyBehavior.SetterRequiresSetup = true;
                }
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="DefaultValueProvider"/> provider of 
        /// default values for the mock.
        /// </summary>
        public DefaultValueProvider DefaultValue
        {
            get => State.GetOrAdd(() => new DefaultValueProvider());
            set
            {
                if (value == null)
                    throw new ArgumentNullException(nameof(value));

                if (State.TryGetValue<DefaultValueProvider>(out var defaultValue) &&
                    value != defaultValue &&
                    Behaviors.OfType<DefaultValueBehavior>().FirstOrDefault() is DefaultValueBehavior behavior &&
                    behavior != null)
                {
                    behavior.Provider = value;
                }

                State.Set(value);
            }
        }
    }
}
