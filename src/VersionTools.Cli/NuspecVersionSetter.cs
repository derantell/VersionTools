using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using VersionTools.Lib;

namespace VersionTools.Cli {
    class NuspecVersionSetter {
        public static void SetVersion(string file, Semver version) {
            if(!File.Exists(file)) return;
            
            Program.VerboseOut(Verbose.Version, "Setting nuspec version {0} of {1}", version.ToString("P"), file);

            var lines    = File.ReadAllLines(file);
            var newLines = new List<string>();

            foreach (var line in lines) {
                if (VersionMatcher.IsMatch(line)) {
                    newLines.Add( VersionMatcher.Replace(line, "${1}" + version.ToString("P") + "$2"));
                } else {
                    newLines.Add(line);
                }
            }

            FileUtil.AssertWritable(file);
            File.WriteAllLines(file, newLines, Encoding.UTF8);
        }

        private static readonly Regex VersionMatcher =
            new Regex(@"(.*?<version>)\s*\S+\s*(</version>.*?)", RegexOptions.Compiled);
    }
}