﻿using System;
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
            VersionArgs.ListAction = HandleListAction;
            VersionArgs.SetAction  = HandleSetAction;
        }

        static void Main(string[] cmdArgs) {
            if (cmdArgs.Length == 0) {
                ArgUsage.GetStyledUsage<VersionArgs>().Write();
                return;
            }

            try {
                Args.InvokeAction<VersionArgs>(cmdArgs);
            }
            catch (ArgException e) {
                Console.Error.Write(e.Message);
            }
        }

        private static void HandleListAction(ListArgs args) {
            if (args.Location == "") {
                args.Location = Directory.GetCurrentDirectory();
            }

            var assemblyEnumerator = new AssemblyEnumerator(args.Location, args.Recurse);

            foreach (var assembly in assemblyEnumerator.GetAssemblies()) {
                DisplayVersion(assembly);
            }
        }


        public static void HandleSetAction(SetArgs args) {
            if (args.Semver != "") {
                var semver = Semver.Parse(args.Semver);
                var locator = new AssemblyInfoLocator(Directory.GetCurrentDirectory(), args.Recurse);

                var version = new Version {
                    Assembly = semver.Version + ".0",
                    File = semver.FullVersion,
                    Product = FormatProductVersion("My lib", semver)
                };

                foreach (var file in locator.LocateAssemblyInfoFiles()) {
                    AssemblyVersionSetter.SetVersion(file.Path, file.VersionContext.Version);
                }
            }
        }

        private static string FormatProductVersion(string product, Semver version) {
            var productVersion = product + " v" + version.Version;
            if (version.IsPreRelease) {
                productVersion += " " + version.PreRelease.Replace('.', ' ');
            }
            return productVersion;
        }

        private static void DisplayVersion(Assembly assembly) {
            Console.WriteLine(assembly.GetName().Name);
            Console.WriteLine("  Location:           {0}", assembly.Location);
            Console.WriteLine("  Assembly version:   {0}", assembly.GetAssemblyVersion());
            Console.WriteLine("  File version:       {0}", assembly.GetAssemblyFileVersion());
            Console.WriteLine("  Product version:    {0}", assembly.GetProductVersion());
            Console.WriteLine();
        }
    }

    class AssemblyVersionSetter {
        public static void SetVersion(string file, Version version) {
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

    class Version {
        public string Assembly { get; set; }
        public string File     { get; set; }
        public string Product  { get; set; }
    }

    class AssemblyInfoLocator {
        public AssemblyInfoLocator( string rootDirectory, bool recurse = false) {
            _recurse = recurse;
            _rootDirectory = rootDirectory;
        }

        public AssemblyVersionFileInfo[] LocateAssemblyInfoFiles(VersionContext versionContext = null) {
            versionContext = versionContext ?? new VersionContext {ProjectName = "", Version = "0.0.0"};
            versionContext = UpdateVersionContext(_rootDirectory, versionContext);

            if (!_recurse) {
                string filepath;

                if (TryGetAssemblyInfoFile(_rootDirectory, out filepath)) {
                    return new[] {new AssemblyVersionFileInfo {
                        Path = filepath, VersionContext = versionContext
                    }};
                }

                if (TryGetAssemblyInfoFile(_rootDirectory + @"\Properties", out filepath)) {
                    return new[] {new AssemblyVersionFileInfo {
                        Path = filepath, VersionContext = versionContext
                    }};
                }
                
                return new AssemblyVersionFileInfo[0];
            }

            var files = new List<AssemblyVersionFileInfo>();
            RecurseFindAssemblyInfo(_rootDirectory, files, versionContext);

            return files.ToArray();
        }

        private void RecurseFindAssemblyInfo(string directory, List<AssemblyVersionFileInfo> files, VersionContext context) {
            var newContext = UpdateVersionContext(directory, context);
            string filepath;
            if (TryGetAssemblyInfoFile(directory, out filepath)) {
                files.Add(new AssemblyVersionFileInfo {
                    Path = filepath,
                    VersionContext = context
                });
            }

            foreach (var dir in Directory.GetDirectories(directory)) {
                RecurseFindAssemblyInfo(dir, files, newContext);
            }
        }

        private bool TryGetAssemblyInfoFile(string directory, out string filePath) {
            if (!Directory.Exists(directory)) {
                filePath = null;
                return false;
            }

            filePath = Directory
                .GetFiles(directory)
                .SingleOrDefault(f => AssemblyInfoCs.Equals( Path.GetFileName(f)));

            return (filePath != null);
        }

        private VersionContext UpdateVersionContext(string directory, VersionContext currentContext) {
            var newContext  = new VersionContext {
                ProjectName = currentContext.ProjectName,
                Version     = currentContext.Version
            };

            var files  = Directory.GetFiles(directory);
            var csproj = files.FirstOrDefault(f => ".csproj".Equals(Path.GetExtension(f)));

            if (csproj != null) {
                newContext.ProjectName = Path.GetFileNameWithoutExtension(csproj);
            }

            var versionFile = files
                .FirstOrDefault(f => "version".Equals(
                    Path.GetFileNameWithoutExtension(f),
                    StringComparison.InvariantCultureIgnoreCase));

            if (versionFile != null) {
                var version = File.ReadAllText(versionFile).Trim();
                newContext.Version = version;
            }

            return newContext;
        }

        private readonly bool   _recurse;
        private readonly string _rootDirectory;
        private const    string AssemblyInfoCs = "AssemblyInfo.cs";
        
    }

    public class AssemblyVersionFileInfo {
        public string Path { get; set; }
        public VersionContext VersionContext { get; set; }
    }
   
    public class VersionContext {
        public string Version { get; set; }
        public string ProjectName { get; set; }
    }

    [ArgExample("ver list MyLib.dll", "Displays the versions of the MyLib.dll assembly")]
    [ArgExample("ver set MyLib.dll -semver 1.3.4", "Sets the version of MyLib.dll to 1.3.4")]
    public class VersionArgs {
        [ArgRequired]
        [ArgPosition(0)]
        [ArgDescription("list to list versions, set to set versions")]
        public string Action { get; set; }

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
            ArgUsage.GetStyledUsage<VersionArgs>().Write();
        }

        public static Action<ListArgs> ListAction { get; set; }
        public static Action<SetArgs>  SetAction  { get; set; }
    }

    public class ListArgs {
        [DefaultValue("")]
        [ArgDescription("The file or directory path from which to read assemblies. " +
                        "Defaults to the current directory")]
        public string Location { get; set; }

        [DefaultValue(false)]
        [ArgDescription("Visit child directories when listing assemblies")]
        public bool Recurse { get; set; }
    }

    public class SetArgs {
        [DefaultValue("")]
        [ArgDescription("The semantic version to set")]
        public string Semver { get; set; }

        [DefaultValue(false)]
        [ArgDescription("Visit child directories when looking for AssemblyInfo files")]
        public bool Recurse { get; set; }

        [DefaultValue("")]
        [ArgDescription("The name of the product the assembly is built for")]
        public string Product { get; set; }
    }
    
    public class HelpArgs {
        [ArgDescription("The bar")]
        public string Bar { get; set; }
    }
}
