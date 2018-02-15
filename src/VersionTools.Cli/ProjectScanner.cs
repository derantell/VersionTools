using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using VersionTools.Lib;

namespace VersionTools.Cli {
    public class ProjectScanner {

        public Project[] Scan(Semver version, bool scan = false ) {
            _scan = scan;
            var projects = new List<Project>();

            ScanDirectory(Program.RootDirectory, projects, version );

            return projects.ToArray();
        }

        private void ScanDirectory(DirectoryInfo directory, List<Project> projects, Semver currentVersion) {
            var versionFile = directory.GetFiles("version.txt").SingleOrDefault();
            if (versionFile != null) {
                Program.VerboseOut(Verbose.Scanning, "Found version.txt at {0}", directory.FullName);
                var parser = new VersionFileParser(versionFile);
                try {
                    var parsedVersion = parser.GetVersion();

                    Program.VerboseOut(Verbose.Scanning, "Parsed version: {0}", parsedVersion);

                    if (Program.RootDirectory.FullName.Equals(directory.FullName) && currentVersion == Semver.NoVersion) {
                        Program.RootVersion = parsedVersion;
                        Program.VerboseOut(Verbose.Scanning, "Setting root version: {0}", parsedVersion);
                    }

                    currentVersion = parsedVersion;
                } catch (FormatException) {
                    Program.VerboseOut(Verbose.Scanning, "Skipping invalid version in file {0}", directory.FullName);
                }
            }

            var nuspecFile = directory.GetFiles("*.nuspec")
                .FirstOrDefault(file => file.Name.StartsWith(directory.Name));
            var projFile   = directory.GetFiles("*.csproj")
                .FirstOrDefault(file => file.Name.StartsWith(directory.Name));

            if (projFile != null || nuspecFile != null) {
                // We say that this is a project if it has either a projfile or a nuspec file.
                var projName = directory.Name;
                Program.VerboseOut(Verbose.Scanning, "Found project {0} at {1}", projName, directory.FullName);

                var project = new Project {
                    Name    = projName,
                    Path    = directory.FullName,
                    Version = currentVersion,
                    Nuspec  = (nuspecFile != null) ? nuspecFile.FullName : null
                };
                    
                projects.Add(project);
            }

            if (_scan) {
                var directories = directory.GetDirectories();
                foreach (var directoryInfo in directories) {
                    try {
                        ScanDirectory(directoryInfo, projects, currentVersion);
                    }
                    catch (PathTooLongException) {
                         Program.VerboseOut(Verbose.Warning, 
                             "Skipping subdir {0}; path too long", directoryInfo.FullName);
                    }
                }
            }
        }

        private bool _scan;
    }
}