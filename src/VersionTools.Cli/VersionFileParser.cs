using System.IO;
using System.Linq;
using VersionTools.Lib;

namespace VersionTools.Cli {
    public class VersionFileParser {
        public VersionFileParser( FileInfo file ) {
            _file = file;
        }
    
        public Semver GetVersion() {
            var line = File.ReadLines(_file.FullName).First( l => !string.IsNullOrWhiteSpace(l));
            return Semver.Parse(line);
        }

        private readonly FileInfo _file;
    }
}