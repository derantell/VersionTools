using System;
using System.Reflection;
using Xunit;

//[assembly: AssemblyVersion("1.2.3.0")]
//[assembly: AssemblyFileVersion("1.2.3-beta2+master.deadb33f")]
//[assembly: AssemblyInformationalVersion("Test project v1.2.3")]

namespace VersionTools.Lib.Test {
    public class VersionHelperTest {
        public class GetAssemblyFileVersion_method {
            [Fact(Skip = "Get some assembly to test on")]
            public void should_return_the_full_file_version_string_of_the_calling_assembly() {
                var fileVersion = VersionHelper.GetAssemblyFileVersion();
                Assert.Equal( "1.2.3-beta2+master.deadb33f", fileVersion );
            }


            [Fact(Skip="Get some assembly to test on")]
            public void should_return_the_full_file_version_of_the_assembly_that_contains_the_specified_type() {
                var fileVersion = VersionHelper.GetAssemblyFileVersion(GetType());
                Assert.Equal("1.2.3-beta2+master.deadb33f", fileVersion);
            }


            [Fact(Skip="Get some assembly to test on")]
            public void should_return_the_full_file_version_of_the_assembly_with_the_specified_path() {
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                var fileVersion = VersionHelper.GetAssemblyFileVersion(assemblyPath);
                Assert.Equal( "1.2.3-beta2+master.deadb33f", fileVersion );
            }


            [Fact(Skip="Get some assembly to test on")]
            public void should_return_the_full_file_version_when_called_as_extension_method_of_Assembly() {
                var fileVersion = Assembly.GetExecutingAssembly().GetAssemblyFileVersion();
                Assert.Equal( "1.2.3-beta2+master.deadb33f", fileVersion );
            }

        }


        public class GetAssemblyVersion_method {
            [Fact(Skip="Get some assembly to test on")]
            public void should_return_the_assembly_version_of_the_calling_assembly() {
                var assemblyVersion = VersionHelper.GetAssemblyVersion();
                Assert.Equal("1.2.3.0", assemblyVersion);
            }


            [Fact(Skip="Get some assembly to test on")]
            public void should_return_the_assembly_version_of_the_assembly_containing_the_specified_type() {
                var assemblyVersion = VersionHelper.GetAssemblyVersion(GetType());
                Assert.Equal("1.2.3.0", assemblyVersion);
            }


            [Fact(Skip="Get some assembly to test on")]
            public void should_return_the_assembly_version_of_the_assembly_at_the_specified_location() {
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                var assemblyVersion = VersionHelper.GetAssemblyVersion(assemblyPath);
                Assert.Equal("1.2.3.0", assemblyVersion );
            }


            [Fact(Skip="Get some assembly to test on")]
            public void should_return_the_assembly_version_of_the_assembly_when_used_as_an_extension_method() {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetAssemblyVersion();
                Assert.Equal("1.2.3.0", assemblyVersion);
                
            }

        }


        public class GetProductVersion_method {
            [Fact(Skip="Get some assembly to test on")]
            public void should_return_the_product_version_of_the_calling_assembly() {
                var productVersion = VersionHelper.GetProductVersion();
                Assert.Equal("Test project v1.2.3", productVersion);
            }


            [Fact(Skip="Get some assembly to test on")]
            public void should_return_the_product_version_of_the_assembly_containing_the_specified_type() {
                var productVersion = VersionHelper.GetProductVersion(typeof(VersionHelper));
                Assert.Equal("1.0.0.0", productVersion);
            }


            [Fact(Skip="Get some assembly to test on")]
            public void should_return_the_product_version_of_the_assembly_at_the_specified_location() {
                var assemblyPath = Assembly.GetExecutingAssembly().Location;
                var productVersion = VersionHelper.GetProductVersion(assemblyPath);
                Assert.Equal("Test project v1.2.3", productVersion);
            }


            [Fact(Skip="Get some assembly to test on")]
            public void should_return_the_product_version_of_the_assembly_when_used_as_extension() {
                var assemblyVersion = Assembly.GetExecutingAssembly().GetProductVersion();
                Assert.Equal("Test project v1.2.3", assemblyVersion);
            }

        }
    }
}