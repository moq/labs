using System;

namespace Moq
{
    /// <summary>
    /// Annotates a method that creates an implicit <see cref="SetupScope"/>.
    /// </summary>
    /// <remarks>
    /// This allows a method like <c>Setup</c> that receives a lambda, to have 
    /// its parameter considered as a setup lambda, so that executing it does 
    /// not cause the actual mock behavior to run, and instead it turns on 
    /// an automatic <see cref="SetupScope"/> so that behaviors can adapt to 
    /// the invocation dynamically.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method)]
    public class SetupScopeAttribute : Attribute
    {
    }
}