namespace Stunts
{
    /// <summary>
    /// The phase at which an <see cref="IDocumentProcessor"/> acts.
    /// </summary>
    public enum ProcessorPhase
    {
        /// <summary>
        /// Initial stunt generation phase where the basic imports, namespace and 
        /// blank stunt class declaration and initial base type and implemented interfaces 
        /// are laid out in the doc. This is typically the phase were additional interfaces 
        /// can be injected to participate in the subsequent phases automatically. 
        /// </summary>
        /// <remarks>
        /// Moq's <c>IMocked</c> interface is registered in this phase, for example.
        /// </remarks>
        Prepare,

        /// <summary>
        /// Phase that generates the basic boilerplate for all abstract and interface members,
        /// that typically have default implementations that basically throw <see cref="NotImplementedException"/>.
        /// For C# and VB, this is achieved by executing the built-in code fixes for abstract 
        /// class and interface default implementations.
        /// </summary>
        Scaffold,

        /// <summary>
        /// Executed right after scaffold, this phase performs the initial stunt implementation rewriting 
        /// by replacing methods that throw <see cref="NotImplementedException"/> generated during scaffold 
        /// and invokes the <see cref="BehaviorPipeline"/> instead for each of them.
        /// </summary>
        Rewrite,

        /// <summary>
        /// Final phase that allows generators to perform additional generation beyond scaffold 
        /// and inital stunt rewriting. Members generated in this phase are not rewritten at all 
        /// to use the <see cref="BehaviorPipeline"/> and can consist of language-specific fixups 
        /// or cleanups to make the generated code more idiomatic than the default code fixes may 
        /// provide.
        /// </summary>
        Fixup,
    }
}
