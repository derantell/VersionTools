using VersionTools.Lib;

namespace VersionTools.Cli {
    public class AssemblyVersion {
        public AssemblyVersion() {
            Assembly = File = Informational = DefaultVersion;
        }

        public string Assembly       { get; set; }
        public string File           { get; set; }
        public string Informational  { get; set; }

        public static AssemblyVersion FromSemver(Semver version) {
            return new AssemblyVersion {
                Assembly      = version.Version + ".0",
                File          = version.Version + ".0",
                Informational = version.FullVersion
            };
        }

        public const string DefaultVersion = "1.0.0.0";
    }
}