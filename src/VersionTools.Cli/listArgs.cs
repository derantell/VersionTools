using PowerArgs;

namespace VersionTools.Cli {
    public class listArgs {
        [ArgDescription("List the versions of any .Net assemblies found in the location(s)")]
        public bool assembly { get; set; }

        [ArgDescription("Visit child directories when listing versions")]
        public bool recurse { get; set; }
    }
}