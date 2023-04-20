using CommandLine;
using Osmy.Api;
using Osmy.Core.Data.Sbom;

namespace Osmy.Cli
{
    internal class Program
    {
        static async Task<int> Main(string[] args)
        {
            try
            {
                return await Parser.Default.ParseArguments<ListOptions, ShowOptions, AddOptions, UpdateOptions, DeleteOptions>(args)
                    .MapResult(
                        (ListOptions opt) => RunListAndReturnExitCodeAsync(opt),
                        (ShowOptions opt) => RunShowAndReturnExitCodeAsync(opt),
                        (AddOptions opt) => RunAddAndReturnExitCodeAsync(opt),
                        (UpdateOptions opt) => RunUpdateAndReturnExitCodeAsync(opt),
                        (DeleteOptions opt) => RunDeleteAndReturnExitCodeAsync(opt),
                        errs => Task.FromResult(1)
                    );
            }
            catch (HttpRequestException)
            {
                Console.Error.WriteLine("Failed to communicate with the server. Please make sure that the server is running.");
                return 1;
            }
        }

        static async Task<int> RunListAndReturnExitCodeAsync(ListOptions opt)
        {
            using var client = new RestClient();
            var fetchTask = client.GetSbomsAsync();
            await ShowProgressAndWait(fetchTask, "Fetching software info...");

            var sbomInfos = (await fetchTask).ToArray();
            var num = sbomInfos.Length;
            if (num == 0) { return 0; }

            var idWidth = Math.Max("ID".Length, (int)Math.Ceiling(Math.Log10(num)));
            var nameWidth = Math.Max("Name".Length, sbomInfos.Max(x => x.Sbom.Name.Length));
            var localDirWidth = Math.Max("Local Directory".Length, sbomInfos.Max(x => x.Sbom.LocalDirectory?.Length) ?? 0);

            var writer = new TableWriter(idWidth, nameWidth, localDirWidth);
            writer.WriteHeader("ID", "Name", "Local Directory");
            foreach (var sbomInfo in sbomInfos)
            {
                writer.WriteRow(sbomInfo.Sbom.Id.ToString(), sbomInfo.Sbom.Name, sbomInfo.Sbom.LocalDirectory);
            }

            return 0;
        }

        static async Task<int> RunShowAndReturnExitCodeAsync(ShowOptions opt)
        {
            using var client = new RestClient();
            var infoTask = client.GetSbomsAsync();
            var vulnsTask = client.GetLatestVulnerabilityScanResultAsync(opt.Id);
            var checksumTask = client.GetLatestChecksumVerificationResultCollectionAsync(opt.Id);
            await ShowProgressAndWait(Task.WhenAll(infoTask, vulnsTask, checksumTask), "Fetching software info...");

            var info = (await infoTask).FirstOrDefault(x => x.Sbom.Id == opt.Id);
            if (info is null)
            {
                Console.Error.WriteLine("Software of specified id was not found.");
                return 1;
            }

            var vulnsScanResult = await vulnsTask;
            var checksumVerificationResult = await checksumTask;
            Console.WriteLine("Name: " + info.Sbom.Name);
            Console.WriteLine("Local Directory: " + info.Sbom.LocalDirectory);

            Console.WriteLine();

            if (vulnsScanResult is null)
            {
                Console.WriteLine("Vulnerability scan has not yet executed.");
            }
            else
            {
                if (vulnsScanResult.IsVulnerable)
                {
                    Console.WriteLine($"{vulnsScanResult.Results.Count(x => x.IsVulnerable)} vulnerabilities detected");
                }
                else
                {
                    Console.WriteLine("No vulnerability detected.");
                }

                var packageNameWidth = Math.Max("Name".Length, vulnsScanResult.Results.Max(x => x.Package.Name.Length));
                var packageVersionWidth = Math.Max("Version".Length, vulnsScanResult.Results.Max(x => x.Package.Version?.Length) ?? 0);
                var writer = new TableWriter(1, packageNameWidth, packageVersionWidth);
                writer.WriteHeader(string.Empty, "Name", "Version");
                foreach (var package in vulnsScanResult.Results)
                {
                    writer.WriteRow(package.IsVulnerable ? "*" : string.Empty, package.Package.Name, package.Package.Version);
                }
            }

            Console.WriteLine();

            if (checksumVerificationResult is null)
            {
                Console.WriteLine("Checksum verificatoin has not yet executed.");
            }
            else
            {
                if (checksumVerificationResult.HasError)
                {
                    var problemCount = checksumVerificationResult.Results.Count(x => x.Result != Core.Data.Sbom.ChecksumVerification.ChecksumCorrectness.Correct);
                    Console.WriteLine($"{problemCount} problem(s) exists.");
                }
                else
                {
                    Console.WriteLine("No problem.");
                }

                var fileNameWidth = Math.Max("File Name".Length, checksumVerificationResult.Results.Max(x => x.SbomFile.FileName.Length));
                var resultWidth = 12;
                var writer = new TableWriter(fileNameWidth, resultWidth);
                writer.WriteHeader("File Name", "Result");
                foreach (var result in checksumVerificationResult.Results)
                {
                    writer.WriteRow(result.SbomFile.FileName, result.Result.ToString());
                }
            }

            return 0;
        }

        private static async Task<int> RunAddAndReturnExitCodeAsync(AddOptions opt)
        {
            using var client = new RestClient();
            var addTask = client.CreateSbomAsync(new AddSbomInfo(opt.Name, opt.SbomFile, opt.LocalDirectory));
            await ShowProgressAndWait(addTask, "Adding software...");

            return 0;
        }

        private static async Task<int> RunUpdateAndReturnExitCodeAsync(UpdateOptions opt)
        {
            using var client = new RestClient();
            var fetchTask = client.GetSbomsAsync();
            await ShowProgressAndWait(fetchTask, "Fetching current info...");
            var current = (await fetchTask).First(x => x.Sbom.Id == opt.Id);

            if (opt.LocalDirectory == "unset")
            {
                opt.LocalDirectory = null;
            }
            else if (opt.LocalDirectory is null)
            {
                opt.LocalDirectory = current.Sbom.LocalDirectory;
            }
            else if (!Directory.Exists(opt.LocalDirectory))
            {
                Console.Error.WriteLine($"{opt.LocalDirectory} does not exists.");
                return 1;
            }

            var updateTask = client.UpdateSbomAsync(opt.Id, new UpdateSbomInfo(opt.Name ?? current.Sbom.Name, opt.LocalDirectory));
            await ShowProgressAndWait(updateTask, "Updating software info...");

            return 0;
        }

        private static async Task<int> RunDeleteAndReturnExitCodeAsync(DeleteOptions opt)
        {
            using var client = new RestClient();
            var deleteTask = client.DeleteSbomAsync(opt.Id);
            await ShowProgressAndWait(deleteTask, "Deleting software...");

            return 0;
        }

        private static async Task ShowProgressAndWait(Task task, string message = "")
        {
            var bars = new char[] { '/', '-', '\\', '|' };
            var (OriginalLeft, OriginalTop) = Console.GetCursorPosition();
            for (uint i = 0; ; i++)
            {
                if (task.IsCanceled || task.IsCompleted || task.IsFaulted)
                {
                    return;
                }
                var content = message + bars[i % bars.Length];
                Console.Write(content);

                await Task.Delay(250);

                Console.SetCursorPosition(OriginalLeft, OriginalTop);
                Console.Write(new string(' ', content.Length));
                Console.SetCursorPosition(OriginalLeft, OriginalTop);
            }
        }
    }

    [Verb("list", HelpText = "List all softwares managed by osmy.")]
    internal class ListOptions { }

    [Verb("show", HelpText = "Show the details of specified software.")]
    internal class ShowOptions
    {
        [Option('i', "id", Required = true)]
        public long Id { get; set; }
    }

    [Verb("add", HelpText = "Add sbom.")]
    internal class AddOptions
    {
        [Option('n', "name", Required = true)]
        public string Name { get; set; } = string.Empty;

        [Option('s', "sbom", Required = true)]
        public string SbomFile { get; set; } = string.Empty;

        [Option('d', "directory", Required = false)]
        public string? LocalDirectory { get; set; }
    }

    [Verb("update", HelpText = "Update software info.")]
    internal class UpdateOptions
    {
        [Option('i', "id", Required = true)]
        public long Id { get; set; }

        [Option('n', "name", Group = "item")]
        public string? Name { get; set; }

        [Option('d', "directory", Group = "item", HelpText = "Specify \"unset\" if you want to unset directory.")]
        public string? LocalDirectory { get; set; }
    }

    [Verb("delete", HelpText = "Delete software info.")]
    internal class DeleteOptions
    {
        [Option('i', "id", Required = true)]
        public long Id { get; set; }
    }
}