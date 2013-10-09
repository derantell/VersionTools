using System;
using System.IO;
using System.Reflection;
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
                if ( Args != null && Args.verbose) {
                    Err(Verbose.Error, "{0}", e);
                } else {
                    Console.Error.WriteLine(e.Message);
                }
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
                foreach (var project in scanner.Scan(Semver.NoVersion, args.recurse)) {
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

        public static Semver RootVersion { get; set; }

        public static void HandleSetAction(setArgs args) {
            RootVersion = args.version.Length > 0 ? Semver.Parse(args.version) : Semver.NoVersion;
            var hasBuild = args.build.Length > 0;
            
            var scanner  = new ProjectScanner();
            var projects = scanner.Scan(RootVersion, args.scan);
            VerboseOut(Verbose.Scanning, "Scan done; found {0} projects", projects.Length);

            if (args.@override && RootVersion == Semver.NoVersion) {
                VerboseOut(Verbose.Warning, 
                    "No root version set; all versions will be overridden with {0}", Semver.NoVersion);
            }

            if (hasBuild) {
                RootVersion = RootVersion.OverrideBuild(args.build);
            }

            if (args.tcbuildno) {
                Console.Out.WriteLine("##teamcity[buildNumber '{0}']", RootVersion);
            }

            foreach (var project in projects) {
                VerboseOut(Verbose.Version, "Versioning project {0}", project.Name);
                if (args.@override && RootVersion != Semver.NoVersion ) {
                    VerboseOut(Verbose.Version, "Overriding version {0} => {1}", project.Version, RootVersion);
                    project.Version = RootVersion;
                }
                if (hasBuild) {
                    VerboseOut(Verbose.Version, "Overriding build {0} => {1}", project.Version.Build, args.build);
                    project.Version = project.Version.OverrideBuild(args.build);
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
}
