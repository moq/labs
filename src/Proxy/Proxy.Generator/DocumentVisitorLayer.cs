namespace Moq.Proxy
{
    /// <summary>
    /// The layer at which an <see cref="IDocumentVisitor"/> acts.
    /// </summary>
    public static class DocumentVisitorLayer
    {
        /// <summary>
        /// Initial proxy generation phase, where the members are laid out with 
        /// default implementations that basically throw <see cref="NotImplementedException"/>.
        /// </summary>
        public const string Scaffold = nameof(Scaffold);

        /// <summary>
        /// First phase after scaffold, which does the initial proxy implementation rewriting 
        /// to replace the methods that throw <see cref="NotImplementedException"/> generated 
        /// during scaffold to invoke the <see cref="BehaviorPipeline"/> instead.
        /// </summary>
        public const string Rewrite = nameof(Rewrite);

        /// <summary>
        /// Final phase that allows generators to perform additional generation beyond scaffold 
        /// and inital proxy rewriting. Members generated in this phase are not rewritten at all 
        /// to use the <see cref="BehaviorPipeline"/>.
        /// </summary>
        public const string Fixup = nameof(Fixup);
    }
}
