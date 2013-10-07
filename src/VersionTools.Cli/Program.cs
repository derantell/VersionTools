using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;
using PowerArgs;
using VersionTools.Lib;

namespace VersionTools.Cli {
    class Program {
        static Program()  {
            ProgramArgs.ListAction = HandleListAction;
            ProgramArgs.SetAction  = HandleSetAction;
        }

        public static ProgramArgs Args { get; set; }

        static int Main(string[] cmdArgs) {
            if (cmdArgs.Length == 0) {
                ArgUsage.GetStyledUsage<ProgramArgs>().Write();
                return 0;
            }

            try {
                Args = PowerArgs.Args.Parse<ProgramArgs>(cmdArgs);
                PowerArgs.Args.InvokeAction<ProgramArgs>(cmdArgs);
                return 0;
            }
            catch (Exception e) {
                Console.Error.Write(e.Message);
                return 1;
            }
        }

        public static void HandleListAction(listArgs args) {
            if (args.assembly) {
                var assemblyEnumerator = new AssemblyEnumerator(RootDirectory.FullName, args.recurse);

                foreach (var assembly in assemblyEnumerator.GetAssemblies()) {
                    DisplayVersion(assembly);
                }
            } else {
                var scanner = new ProjectScanner();
                foreach (var project in scanner.Scan(scan: args.recurse)) {
                    DisplayVersion(project); 
                }
            }
        }

        public static DirectoryInfo RootDirectory {
            get {
                if (string.IsNullOrWhiteSpace(Args.location)) {
                    return new DirectoryInfo(Directory.GetCurrentDirectory());
                }
                return new DirectoryInfo(Args.location);
            }
        }

        public static void HandleSetAction(setArgs args) {
            var version  = args.version.Length > 0 ? Semver.Parse(args.version) : Semver.NoVersion;
            var hasBuild = args.build.Length > 0;
            var scanner  = new ProjectScanner();

            var projects = scanner.Scan(version, args.scan);

            foreach (var project in projects) {
                VerboseOut(Verbose.Version, "Versioning project {0}", project.Name);
                if (version != Semver.NoVersion) {
                    VerboseOut(Verbose.Version, "Overriding version {0} => {1}", project.Version, version);
                    project.Version = version;
                }
                if (hasBuild) {
                    VerboseOut(Verbose.Version, "Overriding build {0} => {1}", project.Version.Build, args.build);
                    project.Version.OverrideBuild(args.build);
                }
                
                AssemblyVersionSetter.SetVersion(project.AssemlyInfo, project.Version);
                NuspecVersionSetter.SetVersion(project.Nuspec, project.Version);
            }
        }

        private static void DisplayVersion(Assembly assembly) {
            Out(Verbose.Assembly, assembly.GetName().Name);
            Out(Verbose.Version,  "  Location:           {0}", assembly.Location);
            Out(Verbose.Version,  "  Assembly version:   {0}", assembly.GetAssemblyVersion());
            Out(Verbose.Version,  "  File version:       {0}", assembly.GetAssemblyFileVersion());
            Out(Verbose.Version,  "  Product version:    {0}", assembly.GetProductVersion());
            Out();
        }

        private static void DisplayVersion(Project project) {
            Out(Verbose.Version, project.Name);
            Out(Verbose.Version,  "  Location:           {0}", project.Path);
            Out(Verbose.Version,  "  Resolved version:   {0}", project.Version);
        }

        public static void Out() {
            Console.WriteLine();
        }

        public static void Out(string verbose, string format, params object[] args) {
            if (Args.verbose) format = verbose + format;
            Console.WriteLine(format, args);
        }


        public static void VerboseOut(string verbose, string format, params object[] args) {
            if (!Args.verbose) return;
            Console.WriteLine(verbose + format, args);
        }

        public static void Err(string verbose, string format, params object[] args) {
            if (Args.verbose) format = verbose + format;
            Console.Error.WriteLine(format, args);
        }
    }

    public struct Verbose {
        public const string Scanning = "[SCANNING]   > ";
        public const string Assembly = "[ASSEMBLY]   > ";
        public const string Error    = "[ERROR]      > ";
        public const string Version  = "[VERSIONING] > ";
    }
    
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

            File.WriteAllLines(file, newLines, Encoding.UTF8);
        }

        private static readonly Regex AttributeMatcher = 
            new Regex(@"\[assembly:\s*Assembly(Informational|File)?Version\("".*?""\)\]",
                RegexOptions.Compiled);
    }

    class NuspecVersionSetter {
        public static void SetVersion(string file, Semver version) {
            if(!File.Exists(file)) return;
            
            Program.VerboseOut(Verbose.Version, "Setting nuspec version {0} of {1}", version.ToString("P"), file);

            var lines    = File.ReadAllLines(file);
            var newLines = new List<string>();

            foreach (var line in lines) {
                if (VersionMatcher.IsMatch(line)) {
                    newLines.Add( VersionMatcher.Replace(line, "${1}" + version.ToString("P") + "$2"));
                } else {
                    newLines.Add(line);
                }
            }

            File.WriteAllLines(file, newLines, Encoding.UTF8);
        }

        private static readonly Regex VersionMatcher =
            new Regex(@"(.*?<version>)\s*\S+\s*(</version>.*?)", RegexOptions.Compiled);
    }

    public class Project {
        public string Name        { get; set; }
        public string Path        { get; set; }
        public Semver Version     { get; set; }
        public string AssemlyInfo { get { return Path + @"\Properties\AssemblyInfo.cs"; }}
        public string Nuspec      { get; set; }
    }

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

    public class ProjectScanner {

        public Project[] Scan(Semver version = null, bool scan = false ) {
            _scan = scan;
            var projects = new List<Project>();

            ScanDirectory(Program.RootDirectory, projects, version ?? Semver.NoVersion);

            return projects.ToArray();
        }

        private void ScanDirectory(DirectoryInfo directory, List<Project> projects, Semver currentVersion) {
            Program.VerboseOut(Verbose.Scanning, "Entering directory {0}", directory.FullName );

            var versionFile = directory.GetFiles("version.txt").SingleOrDefault();
            if (versionFile != null) {
                Program.VerboseOut(Verbose.Scanning, "Found version.txt");
                var parser = new VersionFileParser(versionFile);
                currentVersion = parser.GetVersion();

                Program.VerboseOut(Verbose.Scanning, "Parsed version: {0}", currentVersion);
            }

            var nuspecFile = directory.GetFiles("*.nuspec").SingleOrDefault();
            var projFile   = directory.GetFiles("*.csproj").SingleOrDefault();

            if (projFile != null) {
                Program.VerboseOut(Verbose.Scanning, "Found project {0}", projFile.Name);

                var project = new Project {
                    Name = projFile.Name,
                    Path = projFile.DirectoryName,
                    Version = currentVersion,
                    Nuspec = (nuspecFile != null) ? nuspecFile.FullName : null
                };
                    
                projects.Add(project);
            }

            if (_scan) {
                var directories = directory.GetDirectories();
                foreach (var directoryInfo in directories) {
                    ScanDirectory(directoryInfo, projects, currentVersion);
                }
            }
        }

        private bool _scan;
    }

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


    [ArgExample("aver help", 
                "Displays the documentation")]
    [ArgExample("aver list . -recurse -assembly", 
                "Displays the versions of all assemblies in the current " +
                "directory, including sub directories")]
    [ArgExample(@"aver set c:\projects\MyLib -b ""master.f00beef"" -s",
                "Sets the version of all decendant projects of MyLib using version.txt files " +
                "and overrides any build meta data with 'master.f00beef'.")]
    public class ProgramArgs {
        [ArgRequired]
        [ArgPosition(0)]
        [ArgDescription("<list> to list versions, <set> to set versions")]
        public string Action { get; set; }

        [DefaultValue("")]
        [ArgPosition(1)]
        [ArgDescription("The directory path that will be the root of the action. " +
                        "Defaults to the current directory")]
        public string location { get; set; }

        [ArgDescription("Display verbose output")]
        [ArgShortcut("ver")]
        public bool verbose { get; set; }

        [ArgDescription("List versions of either projects or assemblies in the specified location")]
        public listArgs listArgs { get; set; }

        [ArgDescription("Set version of specified projects")]
        public setArgs setArgs { get; set; }

        [ArgDescription("Displays usage information")]
        public helpArgs helpArgs { get; set; }

        public static void list(listArgs args) {
            ListAction(args);
        }

        public static void set(setArgs args) {
            SetAction(args);
        }

        public static void help(helpArgs args) {
            ArgUsage.GetStyledUsage<ProgramArgs>().Write();
        }

        public static Action<listArgs> ListAction { get; set; }
        public static Action<setArgs>  SetAction  { get; set; }
    }

    public class listArgs {
        [ArgDescription("Visit child directories when listing versions")]
        public bool recurse { get; set; }

        [ArgDescription("List the versions of any .Net assemblies found in the location(s)")]
        public bool assembly { get; set; }
    }

    public class setArgs {
        [DefaultValue("")]
        [ArgDescription("The semantic version to set. When set, this version overrides any version " +
                        "specified in version.txt files.")]
        public string version { get; set; }

        [DefaultValue("")]
        [ArgDescription("Build meta data. When set this value overrides any build meta data set " +
                        "in version.txt files.")]
        public string build { get; set; }

        [ArgDescription("Tells aver to scan the current directory and its decendants for projects")]
        public bool scan { get; set; }
    }
    
    public class helpArgs {}
}
