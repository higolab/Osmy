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
            var nameWidth = Math.Max("Name".Length, sbomInfos.Max(sbom => sbom.Name.Length));
            var localDirWidth = Math.Max("Local Directory".Length, sbomInfos.Max(sbom => sbom.LocalDirectory?.Length) ?? 0);

            Console.WriteLine("V: Has vulnerabilities");
            Console.WriteLine("F: File checksum mismatched or file is missing");
            Console.WriteLine();

            var writer = new TableWriter(2, idWidth, nameWidth, localDirWidth);
            writer.WriteHeader(string.Empty, "ID", "Name", "Local Directory");
            foreach (var sbomInfo in sbomInfos)
            {
                var v = sbomInfo.IsVulnerable == true ? "V" : " ";
                var f = sbomInfo.HasFileError == true ? "F" : " ";
                writer.WriteRow(v + f, sbomInfo.Id.ToString(), sbomInfo.Name, sbomInfo.LocalDirectory);
            }

            return 0;
        }

        static async Task<int> RunShowAndReturnExitCodeAsync(ShowOptions opt)
        {
            using var client = new RestClient();
            var sbomTask = client.GetSbomAsync(opt.Id);
            await ShowProgressAndWait(sbomTask, "Fetching software info...");

            var sbom = await sbomTask;
            if (sbom is null)
            {
                Console.Error.WriteLine("Software of specified id was not found.");
                return 1;
            }

            Console.WriteLine("Name: " + sbom.Name);
            Console.WriteLine("Local Directory: " + sbom.LocalDirectory);

            Console.WriteLine();

            if (sbom.LastVulnerabilityScan is null)
            {
                Console.WriteLine("Vulnerability scan has not yet executed.");
            }
            else
            {
                if (sbom.IsVulnerable)
                {
                    Console.WriteLine($"{sbom.Packages.Count(pkg => pkg.Vulnerabilities.Any())} vulnerabilities detected");
                }
                else
                {
                    Console.WriteLine("No vulnerability detected.");
                }

                if (sbom.Packages.Any())
                {
                    var packageNameWidth = Math.Max("Name".Length, sbom.Packages.Max(x => x.Name.Length));
                    var packageVersionWidth = Math.Max("Version".Length, sbom.Packages.Max(x => x.Version?.Length) ?? 0);
                    var vulnsWidth = Math.Max("Vulnerability".Length, sbom.Packages.Max(x => x.Vulnerabilities.Sum(y => y.Id.Length + 1) - 1));
                    var writer = new TableWriter(1, packageNameWidth, packageVersionWidth, vulnsWidth);
                    writer.WriteHeader(string.Empty, "Name", "Version", "Vulnerability");
                    foreach (var package in sbom.Packages)
                    {
                        var vulns = string.Join(" ", package.Vulnerabilities.Select(x => x.Id));
                        writer.WriteRow(package.Vulnerabilities.Any() ? "*" : string.Empty, package.Name, package.Version, vulns);
                    }
                }
            }

            Console.WriteLine();

            if (sbom.LastFileCheck is null)
            {
                Console.WriteLine("Checksum verificatoin has not yet executed.");
            }
            else
            {
                if (sbom.HasFileError)
                {
                    var problemCount = sbom.Files.Count(file => file.Status != ChecksumCorrectness.Correct);
                    Console.WriteLine($"{problemCount} problem(s) exists.");
                }
                else
                {
                    Console.WriteLine("No problem.");
                }

                if (sbom.Files.Any())
                {
                    var fileNameWidth = Math.Max("File Name".Length, sbom.Files.Max(x => x.FileName.Length));
                    var resultWidth = 12;
                    var writer = new TableWriter(1, fileNameWidth, resultWidth);
                    writer.WriteHeader(string.Empty, "File Name", "Result");
                    foreach (var file in sbom.Files)
                    {
                        var hasError = file.Status != ChecksumCorrectness.Correct ? "*" : string.Empty;
                        writer.WriteRow(hasError, file.FileName, file.ToString());
                    }
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
            var currentSbom = (await fetchTask).First(sbom => sbom.Id == opt.Id);

            if (opt.LocalDirectory == "unset")
            {
                opt.LocalDirectory = null;
            }
            else if (opt.LocalDirectory is null)
            {
                opt.LocalDirectory = currentSbom.LocalDirectory;
            }
            else if (!Directory.Exists(opt.LocalDirectory))
            {
                Console.Error.WriteLine($"{opt.LocalDirectory} does not exists.");
                return 1;
            }

            var updateTask = client.UpdateSbomAsync(opt.Id, new UpdateSbomInfo(opt.Name ?? currentSbom.Name, opt.LocalDirectory));
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