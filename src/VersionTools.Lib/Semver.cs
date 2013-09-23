using System;
using System.Text.RegularExpressions;

namespace VersionTools.Lib {
    public class Semver : IComparable<Semver> {
        public int    Major       { get; private set; }
        public int    Minor       { get; private set; }
        public int    Patch       { get; private set; }
        public string PreRelease  { get; private set; }
        public string Build       { get; private set; }
        public string FullVersion { get; private set; }
        public string Version {
            get { return string.Format("{0}.{1}.{2}", Major, Minor, Patch); }
        }

        public Semver( int major, int minor, int patch, string prerelease = "", string build = "") {
            Major      = major;
            Minor      = minor;
            Patch      = patch;
            PreRelease = prerelease;
            Build      = build;

            FullVersion = string.Format("{0}.{1}.{2}", Major, Minor, Patch);
            if (PreRelease != "") FullVersion += ("-" + PreRelease);
            if (Build      != "") FullVersion += ("+" + Build);
        }

        public Semver() : this(0,0,0) {}

        public static Semver Parse(string value) {
            var tokens = SemverTokenizer.Match(value);

            if (!tokens.Success) {
                throw new FormatException("Invalid Semantic version");
            }

            var semver = new Semver {
                Major      = int.Parse(tokens.Groups["major"].Value),
                Minor      = int.Parse(tokens.Groups["minor"].Value),
                Patch      = int.Parse(tokens.Groups["patch"].Value),
                PreRelease = tokens.Groups["prerelease"].Value,
                Build      = tokens.Groups["buildmetadata"].Value,
                FullVersion = value,
            };

            return semver;
        }

        public int CompareTo(Semver other) {
            if (other       == null)  return 1;
            if (other.Major != Major) return Major.CompareTo(other.Major);
            if (other.Minor != Minor) return Minor.CompareTo(other.Minor);
            if (other.Patch != Patch) return Patch.CompareTo(other.Patch);

            return 0;
        }

        public override string ToString() {
            return FullVersion;
        }

        private static readonly Regex SemverTokenizer = new Regex(
            @"^       ### Semver tokenizer ###
              (?<major>\d+)                         # The major version number
              \.(?<minor>\d+)                       # The minor version number
              \.(?<patch>\d+)                       # The patch number
              (?:-(?<prerelease>[.a-z0-9-]+))?      # Pre-release version, should be further split on dots to determine precedence 
              (?:\+(?<buildmetadata>[.a-z0-9-]+))?  # Build meta data. This field is not used in precendece calculations
            $",
              RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase);
    }
}