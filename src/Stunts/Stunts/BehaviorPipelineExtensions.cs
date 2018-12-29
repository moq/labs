using System;
using System.ComponentModel;

namespace Stunts
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class BehaviorPipelineExtensions
    {
        /// <summary>
        /// Since no <see cref="ExecuteDelegate"/> is provided as a target, this 
        /// defaults to throwing a <see cref="NotImplementedException"/> if no 
        /// behavior returns before reaching the target.
        /// </summary>
        public static IMethodReturn Execute(this BehaviorPipeline pipeline, IMethodInvocation invocation) 
            => pipeline.Invoke(invocation, (input, next) => throw new NotImplementedException(), true);

        /// <summary>
        /// Since no <see cref="ExecuteDelegate"/> is provided as a target, and a value is required to 
        /// return, this defaults to throwing a <see cref="NotImplementedException"/> if no 
        /// behavior returns before reaching the target.
        /// </summary>
        public static T Execute<T>(this BehaviorPipeline pipeline, IMethodInvocation invocation) 
            => (T)pipeline.Invoke(invocation, (input, next) => throw new NotImplementedException(), true).ReturnValue;

        /// <summary>
        /// Since a value is required to return, this executes the pipeline and requests to throw on 
        /// exceptions.
        /// </summary>
        public static T Execute<T>(this BehaviorPipeline pipeline, IMethodInvocation invocation, ExecuteDelegate target) 
            => (T)pipeline.Invoke(invocation, target, true).ReturnValue;

        /// <summary>
        /// Executes and forces an exception, for void methods.
        /// </summary>
        public static void Execute(this BehaviorPipeline pipeline, IMethodInvocation invocation, ExecuteDelegate target) 
            => pipeline.Invoke(invocation, target, true);
    }
}
