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

        static void Main(string[] cmdArgs) {
            if (cmdArgs.Length == 0) {
                ArgUsage.GetStyledUsage<ProgramArgs>().Write();
                return;
            }

            try {
                Args = PowerArgs.Args.Parse<ProgramArgs>(cmdArgs);
                PowerArgs.Args.InvokeAction<ProgramArgs>(cmdArgs);
            }
            catch (Exception e) {
                Console.Error.Write(e.Message);
            }
        }

        private static void HandleListAction(ListArgs args) {
            if (Args.Location == "") {
                Args.Location = Directory.GetCurrentDirectory();
            }

            var assemblyEnumerator = new AssemblyEnumerator(Args.Location, Args.Recurse);

            foreach (var assembly in assemblyEnumerator.GetAssemblies()) {
                DisplayVersion(assembly);
            }
        }

        public static DirectoryInfo RootDirectory {
            get {
                if (string.IsNullOrWhiteSpace(Args.Location)) {
                    return new DirectoryInfo(Directory.GetCurrentDirectory());
                }
                return new DirectoryInfo(Args.Location);
            }
        }

        public static void HandleSetAction(SetArgs args) {
            var version  = args.Version.Length > 0 ? Semver.Parse(args.Version) : Semver.NoVersion;
            var hasBuild = args.Build.Length > 0;
            var scanner  = new ProjectScanner();

            var projects = scanner.Scan();

            foreach (var project in projects) {
                if (project.Version == Semver.NoVersion) {
                    project.Version = version;
                }
                if (hasBuild) {
                    project.Version.OverrideBuild(args.Build);
                }
                var assemblyVersion = AssemblyVersion.FromSemver(project.Version);
                AssemblyVersionSetter.SetVersion(project.AssemlyInfo, assemblyVersion);
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

        

        public static void Out() {
            Console.WriteLine();
        }

        public static void Out(string verbose, string format, params object[] args) {
            if (Args.Verbose) format = verbose + format;
            Console.WriteLine(format, args);
        }


        public static void VerboseOut(string verbose, string format, params object[] args) {
            if (!Args.Verbose) return;
            Console.WriteLine(verbose + format, args);
        }

        public static void Err(string verbose, string format, params object[] args) {
            if (Args.Verbose) format = verbose + format;
            Console.Error.WriteLine(format, args);
        }
    }

    public struct Verbose {
        public const string Scanning = "[SCANNING] > ";
        public const string Assembly = "[ASSEMBLY] > ";
        public const string Error    = "[ERROR]    > ";
        public const string Version  = "[VERSION]  > ";
    }
    
    class AssemblyVersionSetter {
        public static void SetVersion(string file, AssemblyVersion version) {
            if(!File.Exists(file)) return;

            var lines = File.ReadAllLines(file);
            var newLines = new List<string>();

            foreach (var line in lines) {
                newLines.Add( AttributeMatcher.Replace(line, "//$0") );
            }

            newLines.Add("// Assembly versions set by aver.exe");
            newLines.Add("[assembly: AssemblyVersion(\""              + version.Assembly + "\")]");
            newLines.Add("[assembly: AssemblyFileVersion(\""          + version.File     + "\")]");
            newLines.Add("[assembly: AssemblyInformationalVersion(\"" + version.Product  + "\")]");

            File.WriteAllLines(file, newLines, Encoding.UTF8);
        }

        private static readonly Regex AttributeMatcher = 
            new Regex(@"\[assembly:\s*Assembly(Informational|File)?Version\("".*?""\)\]",
                RegexOptions.Compiled);
    }

    public class Project {
        public string Name { get; set; }
        public string Path { get; set; }
        public Semver Version { get; set; }
        public string AssemlyInfo {get { return Path + @"\Properties\AssemblyInfo.cs"; }}
    }

    public class AssemblyVersion {
        public AssemblyVersion() {
            Assembly = File = Product = DefaultVersion;
        }

        public string Assembly { get; set; }
        public string File     { get; set; }
        public string Product  { get; set; }

        public static AssemblyVersion FromSemver(Semver version) {
            return new AssemblyVersion {
                Assembly = version.Version + ".0",
                File = version.FullVersion,
                Product = version.FullVersion
            };
        }

        public const string DefaultVersion = "1.0.0.0";
    }

    public class ProjectScanner {

        public Project[] Scan() {
            var projects = new List<Project>();

            ScanDirectory(Program.RootDirectory, projects);

            return projects.ToArray();
        }

        private void ScanDirectory(DirectoryInfo directory, List<Project> projects ) {
            Program.VerboseOut(Verbose.Scanning, "Entering directory {0}", directory.FullName );
            var projFile = directory.GetFiles("*.csproj").SingleOrDefault();

            if (projFile != null) {
                Program.VerboseOut(Verbose.Scanning, "Found project {0}", projFile.Name);

                var project = new Project {
                    Name = projFile.Name,
                    Path = projFile.DirectoryName,
                    Version = Semver.NoVersion
                };

                var versionFile = directory.GetFiles("version.txt").SingleOrDefault();
                if (versionFile != null) {
                    Program.VerboseOut(Verbose.Scanning, "Found version.txt");
                    var parser = new VersionFileParser(versionFile);
                    var version = parser.GetVersion();

                    Program.VerboseOut(Verbose.Scanning, "Parsed version: {0}", version);
                    project.Version = version;
                }
                    
                projects.Add(project);
            }

            if (Program.Args.Recurse) {
                var directories = directory.GetDirectories();
                foreach (var directoryInfo in directories) {
                    ScanDirectory(directoryInfo, projects);
                }
            }
        }
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


    [ArgExample("ver list MyLib.dll", "Displays the versions of the MyLib.dll assembly")]
    [ArgExample("ver set MyLib.dll -semver 1.3.4", "Sets the version of MyLib.dll to 1.3.4")]
    public class ProgramArgs {
        [ArgRequired]
        [ArgPosition(0)]
        [ArgDescription("<list> to list versions, <set> to set versions")]
        public string Action { get; set; }

        [DefaultValue("")]
        [ArgPosition(1)]
        [ArgDescription("The file or directory path from which to read assemblies. " +
                        "Defaults to the current directory")]
        public string Location { get; set; }

        [ArgDescription("Display verbose output")]
        public bool Verbose { get; set; }

        [DefaultValue(false)]
        [ArgDescription("Visit child directories when listing assemblies or updating versions")]
        public bool Recurse { get; set; }

        [ArgDescription("List versions of specified assemblies")]
        public ListArgs ListArgs { get; set; }

        [ArgDescription("Set version of specified assemblies")]
        public SetArgs SetArgs { get; set; }

        [ArgDescription("Displays usage information")]
        public HelpArgs HelpArgs { get; set; }

        public static void List(ListArgs args) {
            ListAction(args);
        }

        public static void Set(SetArgs args) {
            SetAction(args);
        }

        public static void Help(HelpArgs args) {
            ArgUsage.GetStyledUsage<ProgramArgs>().Write();
        }

        public static Action<ListArgs> ListAction { get; set; }
        public static Action<SetArgs>  SetAction  { get; set; }
    }

    public class ListArgs {}

    public class SetArgs {
        [DefaultValue("")]
        [ArgDescription("The version to set, may either be a valid semantic version or " +
                        "a valid assembly version, e.g. 1.2.3.4")]
        public string Version { get; set; }

        [DefaultValue("")]
        [ArgDescription("The name of the product the assembly is built for")]
        public string Product { get; set; }

        [DefaultValue("")]
        [ArgDescription("Build meta data. May contain ., -, 0-9, a-z")]
        public string Build { get; set; }

        [DefaultValue(true)]
        [ArgDescription("Tells aver to scan the directory(ies) for projects")]
        public bool Scan { get; set; }
    }
    
    public class HelpArgs {}
}
