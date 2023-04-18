using Octokit;
using System.Diagnostics;
using System.IO.Compression;
using System.Runtime.Serialization;
using System.Text.RegularExpressions;

namespace Osmy.Server.Data.Sbom.Spdx
{
    internal class SpdxConverter
    {
        const string ConverterFileName = @"tools-java-jar-with-dependencies.jar";
        static readonly string ConverterPath = Path.Combine(
            Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
            "Osmy",
            ConverterFileName);

        /// <summary>
        /// 指定されたSPDXドキュメントをJSON形式に変換します．
        /// </summary>
        /// <param name="path">SPDXドキュメントのパス</param>
        /// <returns>JSON形式に変換したデータ</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="SpdxConvertException"></exception>
        public static byte[] ConvertToJson(string path)
        {
            if (!File.Exists(ConverterPath)) { throw new FileNotFoundException(null, path); }

            /*
             * 出力先ファイルパスの作成
             * tools-javaは指定したパスに既にファイルが存在するとエラーになるため，
             * 一時ファイルのファイル名にJSON形式のSPDXドキュメントの拡張子.spdx.jsonを付加して出力先ファイルパスとする
             * ファイル名の取得に使用した一時ファイルは不要なので削除する
             */
            var tmpFilePath = Path.GetTempFileName();
            var outputPath = $"{tmpFilePath}.spdx.json";
            try
            {
                File.Delete(tmpFilePath);
            }
            catch { }

            var startInfo = new ProcessStartInfo()
            {
                FileName = "java",
                Arguments = $"-jar \"{ConverterPath}\" Convert \"{path}\" \"{outputPath}\"",
                UseShellExecute = false,
                CreateNoWindow = true,
                RedirectStandardError = true,
            };
            var process = Process.Start(startInfo) ?? throw new SpdxConvertException("failed to start process");
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
            catch (Exception ex)
            {
                throw new SpdxConvertException("failed to convert", ex);
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
        /// 指定されたSPDXドキュメントをJSON形式に変換します．
        /// </summary>
        /// <param name="file">SPDXドキュメント</param>
        /// <returns>JSON形式のデータ</returns>
        /// <exception cref="FileNotFoundException"></exception>
        /// <exception cref="SpdxConvertException"></exception>
        public static byte[] ConvertToJson(IFormFile file)
        {
            var fileName = CreateTempFileWithExtension(Path.GetExtension(file.FileName));
            try
            {
                using var fileStream = file.OpenReadStream();
                using (var tmpFileStream = File.OpenWrite(fileName))
                {
                    fileStream.CopyTo(tmpFileStream);
                }

                return ConvertToJson(fileName);
            }
            finally
            {
                try
                {
                    File.Delete(fileName);
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

        private static string CreateTempFileWithExtension(string extension)
        {
            var tmp = Path.GetTempFileName();
            var fileName = tmp + extension;
            File.Move(tmp, fileName);

            return fileName;
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
