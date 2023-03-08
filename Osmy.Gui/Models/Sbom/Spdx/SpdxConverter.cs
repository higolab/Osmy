using Octokit;
using System;
using System.Diagnostics;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net.Http;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Osmy.Gui.Models.Sbom.Spdx
{
    class SpdxConverter
    {
        const string ConverterFileName = @"tools-java-jar-with-dependencies.jar";
        static readonly string ConverterPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), "Osmy", ConverterFileName);

        public static byte[] ConvertToJson(string path)
        {
            if (!File.Exists(ConverterPath)) { throw new FileNotFoundException(null, path); }

            var outputPath = $"{Path.GetTempFileName()}.spdx.json";

            var startInfo = new ProcessStartInfo()
            {
                FileName = "java",
                Arguments = $"-jar \"{ConverterPath}\" Convert \"{path}\" \"{outputPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
            };
            var process = Process.Start(startInfo);
            if (process is null) { throw new SpdxConvertException("failed to start process"); }
            var error = process.StandardError.ReadToEnd();
            process.WaitForExit();

            if (process.ExitCode != 0)
            {
                throw new SpdxConvertException(error);
            }

            try
            {
                var document = SpdxSerializer.Deserialize(outputPath);

                // 最後のパッケージのHasFilesにすべてのファイルが記載されてしまう不具合の回避処理
                if (document.Packages.Count >= 2)
                {
                    var removeFiles = document.Packages.SkipLast(1).Where(pkg => pkg.HasFiles is not null).SelectMany(pkg => pkg.HasFiles).Distinct().ToArray();
                    var lastPackage = document.Packages.Last();
                    if (lastPackage.HasFiles is not null)
                    {
                        lastPackage.HasFiles = lastPackage.HasFiles.Except(removeFiles).ToList();
                        using var ms = new MemoryStream();
                        SpdxSerializer.Serialize(document, ms);
                        return ms.ToArray();
                    }
                }

                var bytes = File.ReadAllBytes(outputPath);
                return bytes;
            }
            finally
            {
                try
                {
                    File.Delete(outputPath);
                }
                catch { }
            }
        }

        /// <summary>
        /// コンバーターのjarファイルを取得します．
        /// </summary>
        /// <returns>取得に成功すればtrue，それ以外はfalse</returns>
        public static async Task<bool> FetchConverterAsync()
        {
            var pattern = new Regex(@"tools-java-\d+\.\d+\.\d+\.zip");
            var filePattern = new Regex(@"tools-java-\d+\.\d+\.\d+-jar-with-dependencies\.jar");

            if (File.Exists(ConverterPath)) { return true; }

            var gitHubClient = new GitHubClient(new ProductHeaderValue("Osmy"));
            var latestRelease = await gitHubClient.Repository.Release.GetLatest("spdx", "tools-java").ConfigureAwait(false);
            var zipAsset = latestRelease.Assets.FirstOrDefault(x => pattern.IsMatch(x.Name));
            if (zipAsset is null) { return false; }

            using var httpClient = new HttpClient();
            var zipStream = await httpClient.GetStreamAsync(zipAsset.BrowserDownloadUrl).ConfigureAwait(false);
            var extractedDirPath = Path.GetTempFileName();
            File.Delete(extractedDirPath);
            Directory.CreateDirectory(extractedDirPath);
            try
            {
                var zipArchive = new ZipArchive(zipStream);
                zipArchive.ExtractToDirectory(extractedDirPath);

                var jar = Directory.EnumerateFiles(extractedDirPath).FirstOrDefault(x => filePattern.IsMatch(x));
                if (jar is null) { return false; }
                File.Move(jar, ConverterPath);
            }
            finally
            {
                try
                {
                    Directory.Delete(extractedDirPath, true);
                }
                catch { }
            }

            return true;
        }
    }

    internal class SpdxConvertException : Exception
    {
        public SpdxConvertException()
        {
        }

        public SpdxConvertException(string? message) : base(message)
        {
        }

        public SpdxConvertException(string? message, Exception? innerException) : base(message, innerException)
        {
        }

        protected SpdxConvertException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
        }
    }
}
