using System;
using System.Text.RegularExpressions;

namespace VersionTools.Lib {
    public class Semver : IComparable<Semver>, IEquatable<Semver> {
        public int    Major        { get; private set; }
        public int    Minor        { get; private set; }
        public int    Patch        { get; private set; }
        public string PreRelease   { get; private set; }
        public string Build        { get; private set; }
        public string FullVersion  { get; private set; }
        public bool   IsPreRelease {
            get { return !string.IsNullOrWhiteSpace(PreRelease); }
        }
        public string Version      {
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
            var tokens = SemverTokenizer.Match(value ?? "");

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

        public static bool IsValidSemver(string version) {
            return SemverTokenizer.IsMatch(version ?? "");
        }

        public int CompareTo(Semver other) {
            if (other       == null)  return 1;
            if (other.Major != Major) return Major.CompareTo(other.Major);
            if (other.Minor != Minor) return Minor.CompareTo(other.Minor);
            if (other.Patch != Patch) return Patch.CompareTo(other.Patch);

            if ( other.IsPreRelease && !IsPreRelease) return  1;
            if (!other.IsPreRelease &&  IsPreRelease) return -1;

            var selfPre  = PreRelease.Split('.');
            var otherPre = other.PreRelease.Split('.');

            var shortest = Math.Min(selfPre.Length, otherPre.Length);

            for (var i = 0; i < shortest; i++) {
                if (IsNumber(selfPre[i]) && IsNumber(otherPre[i])) {
                    var selfNum  = int.Parse(selfPre [i]);
                    var otherNum = int.Parse(otherPre[i]);
                    return selfNum.CompareTo(otherNum);
                } 
                if (IsNumber(selfPre[i]) ^ IsNumber(otherPre[i]) ) {
                    return IsNumber(selfPre[i]) ? -1 : 1;
                }

                var lexicalOrder = string.Compare(selfPre[i], otherPre[i],  
                    StringComparison.OrdinalIgnoreCase);

                if (lexicalOrder != 0) return (lexicalOrder > 0) ? 1 : -1;
            }

            return selfPre.Length.CompareTo(otherPre.Length);
        }


        public bool Equals(Semver other) {
            return CompareTo(other) == 0;
        }


        public override string ToString() {
            return FullVersion;
        }

        private static bool IsNumber(string value) {
            return Regex.IsMatch(value, @"^(?:0|[1-9]\d*)$");
        }

        private static readonly Regex SemverTokenizer = 
            new Regex(
                @"^       ### Semver tokenizer ###
                  (?<major>\d+)                         # The major version number
                  \.(?<minor>\d+)                       # The minor version number
                  \.(?<patch>\d+)                       # The patch number
                  (?:-(?<prerelease>[.a-z0-9-]+))?      # Pre-release version, should be further split on dots to determine precedence 
                  (?:\+(?<buildmetadata>[.a-z0-9-]+))?  # Build meta data. This field is not used in precendece calculations
                $",
                RegexOptions.Compiled | RegexOptions.IgnorePatternWhitespace | RegexOptions.IgnoreCase
            );
    }
}