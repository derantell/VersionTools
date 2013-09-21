using System;
using System.Reflection;
using Xunit;

[assembly: AssemblyVersion("1.2.3.0")]
[assembly: AssemblyFileVersion("1.2.3-beta2+master.deadb33f")]
[assembly: AssemblyInformationalVersion("Test project v1.2.3")]

namespace VersionTools.Lib.Test {
    public class VersionHelperTest {
        public class GetFileVersion_method {
            [Fact]
            public void should_return_the_full_file_version_string_of_the_calling_assembly() {
                var fileVersion = VersionHelper.GetFileVersion();
                Assert.Equal( "1.2.3-beta2+master.deadb33f", fileVersion );
            }


            [Fact]
            public void should_return_the_full_file_version_of_the_assembly_that_contains_the_specified_type() {
                var fileVersion = VersionHelper.GetFileVersion(typeof (VersionHelper));
                Assert.Equal("1.0.0.0", fileVersion);
            }
        }

        public class GetAssemblyVersion_method {
            [Fact]
            public void should_return_the_assembly_version_of_the_calling_assembly() {
                var assemblyVersion = VersionHelper.GetAssemblyVersion();
                Assert.Equal("1.2.3.0", assemblyVersion);
            }


            [Fact]
            public void should_return_the_assembly_version_of_the_assembly_containing_the_specified_type() {
                var assemblyVersion = VersionHelper.GetAssemblyVersion(typeof (VersionHelper));
                Assert.Equal("1.0.0.0", assemblyVersion);
            }

        }
    }
}