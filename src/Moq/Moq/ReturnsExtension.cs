﻿using System;
using System.ComponentModel;
using System.Linq;
using Castle.Components.DictionaryAdapter;
using Moq.Sdk;
using Stunts;

namespace Moq
{
    /// <summary>
    /// Extensions for configuring return values from mock invocations.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static partial class ReturnsExtension
    {
        /// <summary>
        /// Sets the return value for a property or non-void method.
        /// </summary>
        public static TResult Returns<TResult>(this TResult target, TResult value)
        {
            var setup = MockContext.CurrentSetup;
            if (setup != null)
            {
                var mock = setup.Invocation.Target.AsMock();
                mock.Invocations.Remove(setup.Invocation);
                var behavior = mock.GetPipeline(setup);
                var returnBehavior = behavior.Behaviors.OfType<ReturnsBehavior>().FirstOrDefault();
                if (returnBehavior != null)
                    returnBehavior.Value = value;
                else
                    behavior.Behaviors.Add(new ReturnsBehavior(value));
            }

            return target;
        }

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call.
        /// </summary>
        public static TResult Returns<TResult>(this TResult target, Func<TResult> value)
        {
            var setup = MockContext.CurrentSetup;
            if (setup != null)
            {
                var mock = setup.Invocation.Target.AsMock();
                mock.Invocations.Remove(setup.Invocation);
                var behavior = mock.GetPipeline(setup);
                var returnBehavior = behavior.Behaviors.OfType<ReturnsBehavior>().FirstOrDefault();
                if (returnBehavior != null)
                    returnBehavior.ValueGetter = _ => value();
                else
                    behavior.Behaviors.Add(new ReturnsBehavior(_ => value()));
            }

            return target;
        }

        /// <summary>
        /// Sets the return value for a property or non-void method to 
        /// be evaluated dynamically using the given function on every 
        /// call, while allowing access to all arguments of the invocation, 
        /// including ref/out arguments.
        /// </summary>
        public static TResult Returns<TResult>(this TResult target, Func<IArgumentCollection, TResult> value)
        {
            var setup = MockContext.CurrentSetup;
            if (setup != null)
            {
                var mock = setup.Invocation.Target.AsMock();
                mock.Invocations.Remove(setup.Invocation);
                var behavior = mock.GetPipeline(setup);
                var returnBehavior = behavior.Behaviors.OfType<ReturnsBehavior>().FirstOrDefault();
                if (returnBehavior != null)
                    returnBehavior.ValueGetter = x => value(x);
                else
                    behavior.Behaviors.Add(new ReturnsBehavior(x => value(x)));
            }

            return target;
        }

        /// <summary>
        /// Invokes the given delegate when the methog being set up is invoked, typically used 
        /// to access and set ref/out arguments in a typed fashion. Used in combination 
        /// with <see cref="SetupExtension.Setup{TDelegate}(object, TDelegate)"/>.
        /// </summary>
        /// <typeparam name="TDelegate">The lambda to invoke when the setup method runs.</typeparam>
        /// <param name="target">The setup being performed.</param>
        /// <param name="handler">The lambda to invoke when the setup is matched.</param>
        public static void Returns<TDelegate>(this ISetup<TDelegate> target, TDelegate handler)
        {
            if (handler is not Delegate @delegate)
                throw new ArgumentException(ThisAssembly.Strings.Returns.DelegateExpected, nameof(handler));

            using (new SetupScope())
            {
                // Simulate Any<T> matchers for each member parameter
                var parameters = @delegate.Method.GetParameters();
                object?[] arguments = new object[parameters.Length];
                var defaultValue = new DefaultValueProvider(false);
                for (var i = 0; i < arguments.Length; i++)
                {
                    var parameter = parameters[i];

                    MockSetup.Push(new AnyMatcher(parameter.IsOut ? parameter.ParameterType.GetElementType() : parameter.ParameterType));
                    if (!parameter.IsOut)
                        arguments[i] = defaultValue.GetDefault(parameter.ParameterType);
                }

                target.Delegate.DynamicInvoke(arguments);

                // Now we'd have a setup in place and an actual invocation.
                var setup = MockContext.CurrentSetup;
                if (setup != null)
                {
                    setup.Invocation.Target
                        .AsMock()
                        .GetPipeline(setup)
                        .Behaviors.Add(new ReturnsDelegateBehavior(@delegate));
                }
            }
        }

        private static TResult? Returns<TResult>(Delegate value, ExecuteMockDelegate behavior)
        {
            var setup = MockContext.CurrentSetup;
            if (setup != null)
            {
                // TODO: Is this even necessary given that IntelliSense gives us
                // the right compiler safety already?
                setup.Invocation.EnsureCompatible(value);

                var mock = setup.Invocation.Target.AsMock();
                mock.Invocations.Remove(setup.Invocation);
                var mockBehavior = mock.GetPipeline(setup);

                mockBehavior.Behaviors.Add(new DelegateMockBehavior(behavior, "Returns(() => ...)"));
            }

            return default;
        }
    }
}