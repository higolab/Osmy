using System;
using System.Diagnostics;
using System.IO;
using System.Linq;

namespace Osmy.Models.Sbom.Spdx
{
    class SpdxConverter
    {
        const string ConverterJar = @"tools-java-1.1.3-jar-with-dependencies.jar";

        public static byte[] ConvertToJson(string path)
        {
            var converterPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Osmy", ConverterJar);
            if (!File.Exists(converterPath)) { throw new FileNotFoundException(null, path); }

            //string source = @"D:\rio\Download\tools-java-1.1.3\anacron.Cycle.spdx";
            var outputPath = $"{Path.GetTempFileName()}.spdx.json";

            var startInfo = new ProcessStartInfo()
            {
                FileName = "java",
                Arguments = $"-jar {converterPath} Convert {path} {outputPath}",
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            var process = Process.Start(startInfo)!;
            process.WaitForExit();

            var document = SpdxSerializer.Deserialize(path);

            // 最後のパッケージのHasFilesにすべてのファイルが記載されてしまう不具合の回避処理
            if (document.Packages.Count >= 2)
            {
                var removeFiles = document.Packages.SkipLast(1).SelectMany(pkg => pkg.HasFiles).Distinct().ToArray();
                var lastPackage = document.Packages.Last();
                lastPackage.HasFiles = lastPackage.HasFiles.Except(removeFiles).ToList();
                SpdxSerializer.Serialize(document, path);
            }

            var bytes = File.ReadAllBytes(outputPath);
            File.Delete(outputPath);
            return bytes;
        }
    }
}
