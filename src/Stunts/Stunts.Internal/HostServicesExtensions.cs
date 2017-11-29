using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.CodeAnalysis.Host;
using Microsoft.CodeAnalysis.Host.Mef;

namespace Microsoft.CodeAnalysis
{
    public static class HostServiceExtensions
    {
        public static IEnumerable<Lazy<TExtension, TMetadata>> GetExports<TExtension, TMetadata>(this HostServices services)
        {
            var mef = services as IMefHostExportProvider;
            if (mef != null)
                return mef.GetExports<TExtension, TMetadata>();

            return Enumerable.Empty<Lazy<TExtension, TMetadata>>();
        }

        public static IEnumerable<Lazy<TExtension>> GetExports<TExtension>(this HostServices services)
        {
            var mef = services as IMefHostExportProvider;
            if (mef != null)
                return mef.GetExports<TExtension>();

            return Enumerable.Empty<Lazy<TExtension>>();
        }
    }
}
