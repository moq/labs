using System;

namespace Moq.Proxy
{
    /// <summary>
    /// The layer at which an <see cref="IDocumentVisitor"/> acts.
    /// </summary>
    public static class DocumentVisitorLayer
    {
        public const string Rewrite = nameof(Rewrite);
        /// <summary>
        /// Initial proxy generation phase, where the members are laid out with 
        /// default implementations that basically throw <see cref="NotImplementedException"/>.
        /// </summary>
        public const string Scaffold = nameof(Scaffold);
    }
}
