using System;
using PowerArgs;

namespace VersionTools.Cli {
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
}