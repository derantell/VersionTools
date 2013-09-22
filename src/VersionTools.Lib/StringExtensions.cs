using System.Text.RegularExpressions;

namespace VersionTools.Lib {
    public static class StringExtensions {
        public static bool IsMatch(this string self, string pattern) {
            return Regex.IsMatch(self, pattern);
        } 
    }
}