using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
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
           Console.WriteLine("Set Action not implemented");
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
        [ArgDescription("The foo")]
        public string Foo { get; set; }
    }
    
    public class HelpArgs {
        [ArgDescription("The bar")]
        public string Bar { get; set; }
    }
}
