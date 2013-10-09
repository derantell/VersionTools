using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using VersionTools.Lib;

namespace VersionTools.Cli {
    class AssemblyVersionSetter {
        public static void SetVersion(string file, Semver version) {
            if(!File.Exists(file)) return;

            var assemblyVersion = AssemblyVersion.FromSemver(version);
            Program.VerboseOut(Verbose.Version, "Setting version {0} of {1}", version, file);

            var lines    = File.ReadAllLines(file);
            var newLines = new List<string>();

            foreach (var line in lines) {
                newLines.Add( AttributeMatcher.Replace(line, "//$0") );
            }

            newLines.Add("// Assembly versions set by aver.exe");
            newLines.Add("[assembly: AssemblyVersion(\""              + assemblyVersion.Assembly + "\")]");
            newLines.Add("[assembly: AssemblyFileVersion(\""          + assemblyVersion.File     + "\")]");
            newLines.Add("[assembly: AssemblyInformationalVersion(\"" + assemblyVersion.Informational  + "\")]");

            FileUtil.AssertWritable(file);
            File.WriteAllLines(file, newLines, Encoding.UTF8);
        }

        private static readonly Regex AttributeMatcher = 
            new Regex(@"\[assembly:\s*Assembly(Informational|File)?Version\(\s*"".*?""\s*\)\]",
                      RegexOptions.Compiled);
    }
}