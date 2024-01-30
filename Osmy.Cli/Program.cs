using CommandLine;
using Osmy.Api;
using Osmy.Core.Data.Sbom;
using Osmy.Core.Util;

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
            var sbomInfos = (await ShowProgressAndWait(fetchTask, "Fetching software info...")).ToArray();

            var num = sbomInfos.Length;
            if (num == 0) { return 0; }

            var idWidth = Math.Max("ID".Length, (int)Math.Ceiling(Math.Log10(num)) + 1);
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
            var sbom = await ShowProgressAndWait(sbomTask, "Fetching software info...");
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
                    var vulns = sbom.Packages.SelectMany(x => x.Vulnerabilities);
                    var vulnsWidth = Math.Max("Vulnerability".Length, vulns.Any() ? vulns.Max(x => x.Id.Length) : 0);
                    var writer = new TableWriter(1, packageNameWidth, packageVersionWidth, vulnsWidth);
                    writer.WriteHeader(string.Empty, "Name", "Version", "Vulnerability");
                    foreach (var package in sbom.Packages)
                    {
                        writer.WriteRow(package.Vulnerabilities.Any() ? "*" : string.Empty,
                                        package.Name,
                                        package.Version,
                                        package.Vulnerabilities.ElementAtOrDefault(0)?.Id);
                        if (package.Vulnerabilities.Count() >= 2)
                        {
                            foreach (var vuln in package.Vulnerabilities.Skip(1))
                            {
                                writer.WriteRow(true, string.Empty, string.Empty, string.Empty, vuln.Id);
                            }
                        }
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
                        writer.WriteRow(hasError, file.FileName, file.Status.ToString());
                    }
                }
            }

            return 0;
        }

        private static async Task<int> RunAddAndReturnExitCodeAsync(AddOptions opt)
        {
            using var client = new RestClient();

            opt.SbomFile = Path.GetFullPath(opt.SbomFile);
            if (!File.Exists(opt.SbomFile))
            {
                Console.Error.WriteLine($"{opt.SbomFile} does not exist.");
                return 1;
            }
            if (!SpdxUtil.HasValidExtension(opt.SbomFile))
            {
                Console.Error.WriteLine($"\"{opt.SbomFile}\" is not an SPDX file in a supported format.");
                return 1;
            }

            if (opt.LocalDirectory is not null)
            {
                if (Directory.Exists(opt.LocalDirectory))
                {
                    opt.LocalDirectory = Path.GetFullPath(opt.LocalDirectory);
                }
                else
                {
                    Console.Error.WriteLine($"{opt.LocalDirectory} does not exist.");
                    return 1;
                }
            }

            var addTask = client.CreateSbomAsync(new AddSbomInfo(opt.Name, opt.SbomFile, opt.LocalDirectory));
            var sbom = await ShowProgressAndWait(addTask, "Adding software...");
            if (sbom is null)
            {
                Console.Error.WriteLine("Failed to add software.");
                return 1;
            }

            return 0;
        }

        private static async Task<int> RunUpdateAndReturnExitCodeAsync(UpdateOptions opt)
        {
            using var client = new RestClient();
            var fetchTask = client.GetSbomsAsync();
            var sboms = await ShowProgressAndWait(fetchTask, "Fetching current information...");
            if (sboms is null)
            {
                Console.Error.WriteLine("Failed to fetch software information.");
                return 1;
            }

            var currentSbom = sboms.FirstOrDefault(sbom => sbom.Id == opt.Id);
            if (currentSbom is null)
            {
                Console.Error.WriteLine($"Software with ID:{opt.Id} does not exist.");
                return 1;
            }

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
                Console.Error.WriteLine($"{opt.LocalDirectory} does not exist.");
                return 1;
            }
            else
            {
                opt.LocalDirectory = Path.GetFullPath(opt.LocalDirectory);
            }

            var updateTask = client.UpdateSbomAsync(opt.Id, new UpdateSbomInfo(opt.Name ?? currentSbom.Name, opt.LocalDirectory));
            var sbom = await ShowProgressAndWait(updateTask, "Updating software information...");
            if (sbom is null)
            {
                Console.Error.WriteLine("Failed to update software information.");
                return 1;
            }

            return 0;
        }

        private static async Task<int> RunDeleteAndReturnExitCodeAsync(DeleteOptions opt)
        {
            using var client = new RestClient();
            var deleteTask = client.DeleteSbomAsync(opt.Id);
            var isSuccess = await ShowProgressAndWait(deleteTask, "Deleting software...");
            if (!isSuccess)
            {
                Console.Error.WriteLine("Failed to delete software information.");
                return 1;
            }

            return 0;
        }

        private static async Task<T> ShowProgressAndWait<T>(Task<T> task, string message = "")
        {
            var bars = new char[] { '/', '-', '\\', '|' };
            var (OriginalLeft, OriginalTop) = Console.GetCursorPosition();
            for (uint i = 0; ; i++)
            {
                if (task.IsCanceled || task.IsCompleted || task.IsFaulted)
                {
                    return await task;
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