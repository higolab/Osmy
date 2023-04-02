namespace Osmy.Core.Data.Sbom
{
    /// <summary>
    /// SBOM情報
    /// </summary>
    public class Sbom
    {
        /// <summary>
        /// ID
        /// </summary>
        public long Id { get; set; }

        /// <summary>
        /// 管理名
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// ローカルファイルが存在するディレクトリのパス
        /// </summary>
        public string? LocalDirectory { get; set; }

        /// <summary>
        /// ファイル内容のバイト配列
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// ファイルリスト
        /// </summary>
        public IEnumerable<SbomFile> Files { get; set; }

        /// <summary>
        /// 外部参照
        /// </summary>
        public IEnumerable<ExternalReference> ExternalReferences { get; set; }

        /// <summary>
        /// パッケージリスト
        /// </summary>
        public IEnumerable<SbomPackage> Packages { get; }

        /// <summary>
        /// インスタンスを作成します．
        /// </summary>
        /// <remarks>ORMで使用するために用意しています．</remarks>
        public Sbom()
        {
            Name = string.Empty;
            Content = Array.Empty<byte>();
            Files = Enumerable.Empty<SbomFile>();
            ExternalReferences = Enumerable.Empty<ExternalReference>();
            Packages = Enumerable.Empty<SbomPackage>();
        }

        public Sbom(long id, string name, string? localDirectory, byte[] content, IEnumerable<SbomFile> files, IEnumerable<ExternalReference> externalReferences, IEnumerable<SbomPackage> packages)
        {
            Id = id;
            Name = name;
            LocalDirectory = localDirectory;
            Content = content;
            Files = files;
            ExternalReferences = externalReferences;
            Packages = packages;
        }
    }
}
