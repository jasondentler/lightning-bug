using System.Collections.Generic;
using System.Reflection;

namespace Microsoft.Extensions.DependencyInjection
{
    internal static class AssemblyExtensions
    {
        public static IEnumerable<Assembly> FindAllAssemblies(this Assembly rootAssembly)
        {
            var set = new HashSet<Assembly>();
            FindAllAssemblies(rootAssembly, set);
            return set;
        }

        private static void FindAllAssemblies(Assembly assembly, ISet<Assembly> set)
        {
            if (!set.Add(assembly)) return;

            foreach (var assemblyName in assembly.GetReferencedAssemblies())
            {
                try
                {
                    var referencedAssembly = Assembly.Load(assemblyName);
                    FindAllAssemblies(referencedAssembly, set);
                }
                catch 
                {
                    // NO OP
                }
            }
        }

    }
}