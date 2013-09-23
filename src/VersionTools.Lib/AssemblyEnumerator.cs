using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace VersionTools.Lib {
    public class AssemblyEnumerator {
        public AssemblyEnumerator(string location, bool recurse = false) {
            _directory = location;
            _recurse = recurse;
        }

        public IEnumerable<Assembly> GetAssemblies() {
            var assemblies = new HashSet<Assembly>();
            GetAssemblies(assemblies, _directory);
            return assemblies;
        }

        private void GetAssemblies(ISet<Assembly> assemblies, string directory) {
            var files = Directory
                .GetFiles(directory)
                .Where(f=>Path.GetExtension(f).IsMatch(@"^\.(exe|dll)$"));

            foreach (var file in files) {
                try {
                    var assembly = Assembly.ReflectionOnlyLoadFrom(file);
                    assemblies.Add(assembly);
                }
                catch {
                    // TODO: Can we test the dotnetness of a dll/exe in a better way?
                }
            }

            if (_recurse) {
                var directories = Directory.GetDirectories(directory);
                foreach (var dir in directories) {
                    GetAssemblies(assemblies, dir);
                }
            }
        } 

        private readonly string _directory;
        private readonly bool   _recurse;
    }
}