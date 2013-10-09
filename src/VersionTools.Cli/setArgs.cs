using PowerArgs;

namespace VersionTools.Cli {
    public class setArgs {
        [DefaultValue("")]
        [ArgDescription("Build meta data. When set this value overrides any build meta data set " +
                        "in version.txt files.")]
        public string build { get; set; }

        [ArgDescription("Tells aver to override versions specified in version.txt files with the " +
                        "version specified by the version argument")]
        public bool @override { get; set; }
        
        [ArgDescription("Tells aver to scan the current directory and its decendants for projects")]
        public bool scan { get; set; }

        [ArgDescription("Outputs the root version as a teamcity service message")]
        public bool tcbuildno { get; set; }

        [DefaultValue("")]
        [ArgDescription("The semantic version to set. When the -override switch is set, this version" +
                        "overrides any version.txt file")]
        public string version { get; set; }
    }
}