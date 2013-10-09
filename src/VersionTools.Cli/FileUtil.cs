using System.IO;

namespace VersionTools.Cli {
    class FileUtil {
        public static void AssertWritable(string filePath) {
            var attributes = File.GetAttributes(filePath);
            if ((attributes & FileAttributes.ReadOnly) == FileAttributes.ReadOnly) {
                File.SetAttributes(filePath, attributes & ~FileAttributes.ReadOnly);
            }
        }
    }
}