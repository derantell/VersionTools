using System;
using System.Reflection;
using PowerArgs;
using VersionTools.Lib;

namespace VersionTools.Cli {
    class Program {
        static void Main(string[] args) {
            var config = Args.Parse<Arguments>(args);

            string line;

            while (null != (line = Console.ReadLine())) {
                var assembly = Assembly.ReflectionOnlyLoadFrom(line);
                Console.WriteLine("Assembly:           {0}", assembly.GetName().Name);
                Console.WriteLine("Assembly version:   {0}", assembly.GetAssemblyVersion());
                Console.WriteLine("File version:       {0}", assembly.GetAssemblyFileVersion());
                Console.WriteLine("Product version:    {0}", assembly.GetProductVersion());
                Console.WriteLine();
            }

            if (args.Length == 0 || config.Help) {
                Console.WriteLine( ArgUsage.GetStyledUsage<Arguments>());
                return;
            }
        }



        private static bool IsPiped() {
            try {
                var isPiped = System.Console.KeyAvailable;
                return false;
            }
            catch (InvalidOperationException e) {
                return true;
            }
        }
    }

    [ArgExample(@"ver -location c:\path\to\assembly.dll", "Displays the assembly version")]
    class Arguments {
        [ArgDescription("The location of the assembly from which to extract the versions")]
        public string Location { get; set; }

        [ArgDescription("Shows this help")]
        public bool Help { get; set; }
    }
}
