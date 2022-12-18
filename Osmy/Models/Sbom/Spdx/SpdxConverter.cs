using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
            string outputPath = $"{path}.{Guid.NewGuid()}.spdx.json";

            var startInfo = new ProcessStartInfo()
            {
                FileName = "java",
                Arguments = $"-jar {converterPath} Convert {path} {outputPath}",
                UseShellExecute = false,
                CreateNoWindow = true,
            };
            var process = Process.Start(startInfo)!;
            process.WaitForExit();

            // TODO RootPackageとFilesセクションの修復

            var bytes = File.ReadAllBytes(outputPath);
            File.Delete(outputPath);
            return bytes;
        }
    }
}
