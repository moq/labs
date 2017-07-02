namespace Moq.Proxy
{
    /// <summary>
    /// The layer at which an <see cref="IDocumentVisitor"/> acts.
    /// </summary>
    public static class DocumentVisitorLayer
    {
        /// <summary>
        /// Initial proxy generation phase where the basic imports, namespace and 
        /// blank proxy class declaration and initial base type and implemented interfaces 
        /// are laid out in the doc. This is typically the phase were additional interfaces 
        /// can be injected to participate in the subsequent phases automatically. 
        /// </summary>
        /// <remarks>
        /// Moq's <c>IMocked</c> interface is registered in this phase, for example.
        /// </remarks>
        public const string Prepare = nameof(Prepare);

        /// <summary>
        /// Phase that generates the basic boilerplate for all abstract and interface members,
        /// that typically have default implementations that basically throw <see cref="NotImplementedException"/>.
        /// For C# and VB, this is achieved by executing the built-in code fixes for abstract 
        /// class and interface default implementations.
        /// </summary>
        public const string Scaffold = nameof(Scaffold);

        /// <summary>
        /// Executed right after scaffold, this phase performs the initial proxy implementation rewriting 
        /// by replacing methods that throw <see cref="NotImplementedException"/> generated during scaffold 
        /// and invokes the <see cref="BehaviorPipeline"/> instead for each of them.
        /// </summary>
        public const string Rewrite = nameof(Rewrite);

        /// <summary>
        /// Final phase that allows generators to perform additional generation beyond scaffold 
        /// and inital proxy rewriting. Members generated in this phase are not rewritten at all 
        /// to use the <see cref="BehaviorPipeline"/> and can consist of language-specific fixups 
        /// or cleanups to make the generated code more idiomatic than the default code fixes may 
        /// provide.
        /// </summary>
        public const string Fixup = nameof(Fixup);
    }
}
