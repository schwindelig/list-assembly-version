using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ListAssemblyVersions.cs
{
    public struct Arguments
    {
        public string Directory { get; set; }

        public bool IsRecursive { get; set; }

        public string CsvDestination { get; set; }

        public static Arguments FromConsoleArguments(string[] args)
        {
            var arguments = new Arguments();

            if (args?.Length >= 1)
            {
                arguments.Directory = args[0];
            }

            if (args?.Length >= 2 && bool.TryParse(args[1], out var isRecursive))
            {
                arguments.IsRecursive = isRecursive;
            }

            if (args?.Length >= 3)
            {
                arguments.CsvDestination = args[2];
            }

            return arguments;
        }
    }

    public class AssemblyLister
    {
        public virtual IEnumerable<Assembly> GetAssemblies(string directory, bool isRecursive)
        {
            if (string.IsNullOrWhiteSpace(directory)) return null;

            IEnumerable<string> paths;
            try
            {
                paths = Directory.GetFiles(directory, "*.dll",
                    isRecursive ? SearchOption.AllDirectories : SearchOption.TopDirectoryOnly).ToList();
            }
            catch
            {
                // TODO: Add some proper logging here
                return null;
            }

            var assemblies = new List<Assembly>();
            foreach (var path in paths)
            {
                try
                {
                    assemblies.Add(Assembly.LoadFile(path));
                }
                catch
                {
                    // TODO: Add some proper logging here
                }
            }

            return assemblies;
        }

        public virtual void DumpInfosToCsv(IEnumerable<Assembly> assemblies, string destination)
        {
            if (assemblies == null || string.IsNullOrWhiteSpace(destination)) return;

            var stringBuilder = new StringBuilder();
            stringBuilder.AppendLine("Name; FileVersion; ProductVersion");
            foreach (var assembly in assemblies)
            {
                var fileVersionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
                stringBuilder.AppendLine($"{assembly.GetName().Name}; {fileVersionInfo.FileVersion}; {fileVersionInfo.ProductVersion}");
            }

            File.WriteAllText(destination, stringBuilder.ToString());
        }
    }

    public class Program
    {
        static void Main(string[] args)
        {
            var arguments = Arguments.FromConsoleArguments(args);
            var assemblyLister = new AssemblyLister();

            var assemblies = assemblyLister.GetAssemblies(arguments.Directory, arguments.IsRecursive);
            assemblyLister.DumpInfosToCsv(assemblies, arguments.CsvDestination);
        }
    }
}
