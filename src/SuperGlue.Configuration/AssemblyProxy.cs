using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace SuperGlue.Configuration
{
    public class AssemblyProxy : MarshalByRefObject
    {
        private static readonly ICollection<string> LoadedAssemblies = new List<string>();

        public AssemblyProxy()
        {
            AppDomain.CurrentDomain.AssemblyResolve += CurrentDomainAssemblyResolve;
        }

        public void MakeSureAssemblyIsLoaded(string assemblyPath)
        {
            try
            {
                var fileName = Path.GetFileName(assemblyPath);

                if (string.IsNullOrEmpty(fileName))
                    return;

                if (AppDomain.CurrentDomain.SetupInformation.PrivateBinPath.Split(';').Any(x => File.Exists(Path.Combine(x, fileName))))
                    return;

                LoadedAssemblies.Add(assemblyPath);
                Assembly.LoadFile(assemblyPath);
            }
            catch (Exception)
            {
                
            }
        }

        public override object InitializeLifetimeService()
        {
            return null;
        }

        private static Assembly CurrentDomainAssemblyResolve(object sender, ResolveEventArgs args)
        {
            var assemblyPath = LoadedAssemblies.FirstOrDefault(x =>
            {
                var assemblyName = AssemblyName.GetAssemblyName(x);

                return assemblyName.FullName == args.Name || assemblyName.FullName.Split(',')[0] == args.Name;
            });

            return string.IsNullOrEmpty(assemblyPath) ? null : Assembly.LoadFile(assemblyPath);
        }
    }
}