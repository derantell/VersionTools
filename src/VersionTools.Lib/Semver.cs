using System;
using System.Text.RegularExpressions;

namespace VersionTools.Lib {
    public struct Semver : IComparable<Semver>, IEquatable<Semver> {
        public readonly int Major;
        public readonly int Minor;
        public readonly int Patch;
        public readonly string PreRelease;
        public readonly string Build;

        public string FullVersion {
            get {
                var fullVersion = Version;
                if (PreRelease != "") fullVersion += ("-" + PreRelease);
                if (Build      != "") fullVersion += ("+" + Build);
                return fullVersion;
            }
        }
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
        }

        public static Semver Parse(string value) {
            var tokens = SemverTokenizer.Match(value ?? "");

            if (!tokens.Success) {
                throw new FormatException("Invalid Semantic version");
            }

            var semver = new Semver (
                int.Parse(tokens.Groups["major"].Value),
                int.Parse(tokens.Groups["minor"].Value),
                int.Parse(tokens.Groups["patch"].Value),
                tokens.Groups["prerelease"].Value,
                tokens.Groups["buildmetadata"].Value
            );

            return semver;
        }

        public Semver OverrideBuild( string newBuild ) {
            return new Semver( Major, Minor, Patch, PreRelease, newBuild );
        }

        public static bool IsValidSemver(string version) {
            return SemverTokenizer.IsMatch(version ?? "");
        }

        public int CompareTo(Semver other) {
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

        public override bool Equals(object obj) {
            if (obj == null)
                return false;
            if (obj.GetType() != typeof (Semver))
                return false;
            return Equals((Semver) obj);
        }

        public override int GetHashCode() {
            return ToString("P").GetHashCode();
        }

        public static bool operator ==(Semver ver1, Semver ver2) {
            return ver1.Equals(ver2);
        }

        public static bool operator !=(Semver ver1, Semver ver2) {
            return !ver1.Equals(ver2);
        }

        public static bool operator >(Semver ver1, Semver ver2) {
            return ver1.CompareTo(ver2) > 0;
        }

        public static bool operator <(Semver ver1, Semver ver2) {
            return ver1.CompareTo(ver2) < 0;
        }

        public static bool operator >=(Semver ver1, Semver ver2) {
            return ver1.CompareTo(ver2) >= 0;
        }

        public static bool operator <=(Semver ver1, Semver ver2) {
            return ver1.CompareTo(ver2) <= 0;
        }

        public override string ToString() {
            return ToString(null);
        }
        
        /// <summary>
        /// Returns the string representation of the Semver.
        /// </summary>
        /// <param name="format">A format string. Valid values are: <br/>
        /// F - Full version including prerelase and build meta data
        /// P - Pre-release version, excluding build meta data
        /// V - Only normal version, excluding pre-relase version and build meta data</param>
        /// <returns>A string on the specified format</returns>
        public string ToString(string format) {
            if (string.IsNullOrEmpty(format) || format == "F")
                return FullVersion;
            if (format == "P" ) 
                return Version + (IsPreRelease ? ("-" + PreRelease) : "");
            if (format == "V")
                return Version;

            throw new FormatException( 
                string.Format("'{0}' is not a valid semver string format", format));
        }


        public static readonly Semver NoVersion = new Semver(0,0,0);

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