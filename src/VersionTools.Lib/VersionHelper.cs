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
            return GetAssemblyFileVersion(assembly.Location);
        }

        /// <summary>
        /// Gets the file version of the assembly at the specified location.
        /// </summary>
        public static string GetAssemblyFileVersion(string location) {
            var versionInfo = FileVersionInfo.GetVersionInfo(location);
            return versionInfo.FileVersion;
        }

        /// <summary>
        /// Gets the file version of the assembly.
        /// </summary>
        public static string GetAssemblyFileVersion(this Assembly assembly) {
            return GetAssemblyFileVersion(assembly.Location);
        }

        /// <summary>
        /// Gets the assembly version of the assembly. When no type is passed, the version of 
        /// the calling assembly is used. Otherwise the version of the assembly that contains 
        /// the type is returned.
        /// </summary>
        public static string GetAssemblyVersion(Type type = null) {
            var assembly = type != null ? type.Assembly : Assembly.GetCallingAssembly();
            return assembly.GetAssemblyVersion();
        }

        /// <summary>
        /// Tries to load the assembly specified by the location argument and returns its assembly 
        /// version.
        /// </summary>
        public static string GetAssemblyVersion(string location) {
            var assembly = Assembly.ReflectionOnlyLoadFrom(location);
            return assembly.GetAssemblyVersion();
        }

        /// <summary>
        /// Gets the assembly 
        /// </summary>
        /// <param name="assembly"></param>
        /// <returns></returns>
        public static string GetAssemblyVersion(this Assembly assembly) {
            var assemblyVersion = assembly.GetName().Version.ToString();
            return assemblyVersion;
        }

        /// <summary>
        /// Gets the product version of the calling assembly when no type is passed as an argument. 
        /// When a type is passed, that type's containing version is returned.
        /// </summary>
        public static string GetProductVersion(Type type = null) {
            var assembly = type != null ? type.Assembly : Assembly.GetCallingAssembly();
            return GetProductVersion(assembly.Location);
        }

        /// <summary>
        /// Gets the product version of the assembly located at the specified path.
        /// </summary>
        public static string GetProductVersion(string location) {
            var assembly = Assembly.ReflectionOnlyLoadFrom(location);
            var info = FileVersionInfo.GetVersionInfo(assembly.Location);
            return info.ProductVersion;
        }

        /// <summary>
        /// Gets the product version of the specified assembly.
        /// </summary>
        public static string GetProductVersion(this Assembly self) {
            return GetProductVersion(self.Location);
        }
    }
}