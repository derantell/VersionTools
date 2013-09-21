using System;
using System.Diagnostics;
using System.Reflection;

namespace VersionTools.Lib {
    public static class VersionHelper {
        /// <summary>
        /// Gets the file version of the calling assembly when no type is passed as an argument. 
        /// When a type is passed, that type's containing version is returned.
        /// </summary>
        public static string GetAssemblyFileVersion(Type type = null) {
            var assembly = type != null ? type.Assembly : Assembly.GetCallingAssembly();
            var versionInfo = FileVersionInfo.GetVersionInfo(assembly.Location);
            return versionInfo.FileVersion;
        }

        /// <summary>
        /// Gets the assembly version of the assembly. When no type is passed, the version of 
        /// the calling assembly is used. Otherwise the version of the assembly that contains 
        /// the type is returned.
        /// </summary>
        public static string GetAssemblyVersion(Type type = null) {
            var assembly = type != null ? type.Assembly : Assembly.GetCallingAssembly();
            var assemblyVersion = assembly.GetName().Version.ToString();
            return assemblyVersion;
        }
    }
}