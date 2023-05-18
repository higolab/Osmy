namespace Osmy.Core.Data.Sbom
{
    /// <summary>
    /// SBOM情報
    /// </summary>
    public class Sbom : NotificationObject
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }
        private long _id;

        /// <summary>
        /// 管理名
        /// </summary>
        public string Name
        {
            get => _name;
            set => SetProperty(ref _name, value);
        }
        private string _name = string.Empty;

        /// <summary>
        /// ローカルファイルが存在するディレクトリのパス
        /// </summary>
        public string? LocalDirectory
        {
            get => _localDirectory;
            set => SetProperty(ref _localDirectory, value);
        }
        private string? _localDirectory;

        /// <summary>
        /// ファイル内容のバイト配列
        /// </summary>
        public byte[] Content
        {
            get => _content;
            set => SetProperty(ref _content, value);
        }
        private byte[] _content = Array.Empty<byte>();

        /// <summary>
        /// 最終脆弱性診断実行日時
        /// </summary>
        public DateTime? LastVulnerabilityScan { get; set; }

        /// <summary>
        /// 脆弱性が存在するか
        /// </summary>
        public bool IsVulnerable { get; set; }

        /// <summary>
        /// 最終チェックサム検証日時
        /// </summary>
        public DateTime? LastFileCheck { get; set; }

        /// <summary>
        /// ファイルエラーが存在するか
        /// </summary>
        public bool HasFileError { get; set; }

        /// <summary>
        /// ファイルリスト
        /// </summary>
        public IEnumerable<SbomFile> Files
        {
            get => _files;
            set => SetProperty(ref _files, value);
        }
        private IEnumerable<SbomFile> _files = Enumerable.Empty<SbomFile>();

        /// <summary>
        /// パッケージリスト
        /// </summary>
        public IEnumerable<SbomPackage> Packages
        {
            get => _packages;
            set => SetProperty(ref _packages, value);
        }
        private IEnumerable<SbomPackage> _packages = Enumerable.Empty<SbomPackage>();

        /// <summary>
        /// 外部参照
        /// </summary>
        public IEnumerable<ExternalReference> ExternalReferences
        {
            get => _externalReferences;
            set => SetProperty(ref _externalReferences, value);
        }
        private IEnumerable<ExternalReference> _externalReferences = Enumerable.Empty<ExternalReference>();

        /// <summary>
        /// インスタンスを作成します．
        /// </summary>
        /// <remarks>ORMで使用するために用意しています．</remarks>
        public Sbom() { }

        public Sbom(long id,
                    string name,
                    string? localDirectory,
                    DateTime? lastVulnerabilityScan,
                    bool isVulnerable,
                    DateTime? lastFileCheck,
                    bool hasFileError,
                    IEnumerable<SbomFile> files,
                    IEnumerable<ExternalReference> externalReferences,
                    IEnumerable<SbomPackage> packages)
        {
            Id = id;
            Name = name;
            LocalDirectory = localDirectory;
            LastVulnerabilityScan = lastVulnerabilityScan;
            IsVulnerable = isVulnerable;
            LastFileCheck = lastFileCheck;
            HasFileError = hasFileError;
            Files = files;
            ExternalReferences = externalReferences;
            Packages = packages;
        }
    }
}
