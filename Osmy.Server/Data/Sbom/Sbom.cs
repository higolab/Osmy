using Osmy.Server.Data.Sbom.Spdx;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Osmy.Server.Data.Sbom
{
    /// <summary>
    /// SBOM情報
    /// </summary>
    public sealed class Sbom
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
        /// SBOMの形式
        /// </summary>
        public SbomFormat Format { get; set; }

        /// <summary>
        /// URI
        /// </summary>
        public Uri Uri { get; set; }

        /// <summary>
        /// ローカルファイルが存在するディレクトリのパス
        /// </summary>
        public string? LocalDirectory { get; set; }

        /// <summary>
        /// ファイル内容
        /// </summary>
        public RawSbom RawSbom { get; set; }

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
        public List<SbomFileComponent> Files { get; set; }

        /// <summary>
        /// パッケージリスト
        /// </summary>
        public List<SbomPackageComponent> Packages { get; set; }

        /// <summary>
        /// 外部参照
        /// </summary>
        public List<SbomExternalReferenceComponent> ExternalReferences { get; set; }

        /// <summary>
        /// ルートパッケージリスト
        /// </summary>
        [NotMapped]
        public IEnumerable<SbomPackageComponent> RootPackages => Packages.Where(x => x.IsRootPackage);

        /// <summary>
        /// インスタンスを作成します．
        /// </summary>
        /// <remarks>ORMで使用するために用意しています．</remarks>
        public Sbom()
        {
            Name = string.Empty;
            RawSbom = default!;
            Uri = default!;
            Files = new List<SbomFileComponent>();
            Packages = new List<SbomPackageComponent>();
            ExternalReferences = new List<SbomExternalReferenceComponent>();
        }

        /// <summary>
        /// 指定したコンテンツからインスタンスを作成します．
        /// </summary>
        /// <param name="name"></param>
        /// <param name="file"></param>
        /// <param name="localDirectory"></param>
        /// <remarks>新規追加時に呼び出されます．</remarks>
        public Sbom(string name, IFormFile file, string? localDirectory)
        {
            Name = name;
            Format = SbomFormat.Spdx_2_2_Json;
            RawSbom = new RawSbom(SpdxConverter.ConvertToJson(file));
            LocalDirectory = localDirectory;

            ParseSpdx();
        }

        [MemberNotNull(nameof(Uri), nameof(Files), nameof(Packages), nameof(ExternalReferences))]
        private void ParseSpdx()
        {
            using var stream = new MemoryStream(RawSbom.Data);
            var spdxContent = new SpdxDocumentParseHelper(this, stream);

            Uri = spdxContent.Uri;

            Files = spdxContent.Files;
            Packages = spdxContent.Packages;
            ExternalReferences = spdxContent.ExternalReferences;
        }
    }

    public enum SbomFormat
    {
        Spdx_2_2_Json,
    }
}
