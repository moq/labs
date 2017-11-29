using System;

namespace Stunts
{
    /// <summary>
    /// Annotates a method that is a factory for stunts, so that a 
    /// compile-time or design-time generator can generate them ahead of time.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method)]
    public class StuntGeneratorAttribute : Attribute
    {
    }
}