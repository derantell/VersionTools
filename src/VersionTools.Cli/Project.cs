using VersionTools.Lib;

namespace VersionTools.Cli {
    public class Project {
        public string Name        { get; set; }
        public string Path        { get; set; }
        public Semver Version     { get; set; }
        public string AssemlyInfo { get { return Path + @"\Properties\AssemblyInfo.cs"; }}
        public string Nuspec      { get; set; }
    }
}