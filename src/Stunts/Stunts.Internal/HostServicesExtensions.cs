using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Reflection;
using Microsoft.CodeAnalysis.Host;

namespace Microsoft.CodeAnalysis
{
    [EditorBrowsable(EditorBrowsableState.Advanced)]
    public static class HostServiceExtensions
    {
        static readonly ConcurrentDictionary<Tuple<Type, Type, Type>, Delegate> getExportsCache = new ConcurrentDictionary<Tuple<Type, Type, Type>, Delegate>();

        public static IEnumerable<Lazy<TExtension, TMetadata>> GetExports<TExtension, TMetadata>(this HostServices services)
        {
            var getExports = getExportsCache.GetOrAdd(Tuple.Create(services.GetType(), typeof(TExtension), typeof(TMetadata)),
                _ =>
                {
                    var method = services.GetType()
                        .GetMethods(BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic)
                        .Where(m => m.Name.EndsWith("GetExports") &&
                                    m.IsGenericMethodDefinition &&
                                    m.GetGenericArguments().Length == 2)
                        .FirstOrDefault();

                    Func<IEnumerable<Lazy<TExtension, TMetadata>>> func;

                    if (method == null)
                    {
                        func = () => Enumerable.Empty<Lazy<TExtension, TMetadata>>();
                    }
                    else
                    {
                        var generic = method.MakeGenericMethod(typeof(TExtension), typeof(TMetadata));
                        func = () => (IEnumerable<Lazy<TExtension, TMetadata>>)generic.Invoke(services, null);
                    }

                    return func;
                });

            return ((Func<IEnumerable<Lazy<TExtension, TMetadata>>>)getExports).Invoke();
        }
    }
}
