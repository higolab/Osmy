using Osmy.Views;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Security.Cryptography;
using System.Threading.Tasks;

namespace Osmy.Models.Sbom
{
    /// <summary>
    /// SBOM情報
    /// </summary>
    public abstract class Sbom
    {
        /// <summary>
        /// ID
        /// </summary>
        public int Id { get; set; }

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
        public List<SbomFile> Files { get; set; }

        /// <summary>
        /// ルートパッケージリスト
        /// </summary>
        [NotMapped]
        public abstract IReadOnlyCollection<SbomPackage> RootPackages { get; }

        /// <summary>
        /// パッケージリスト
        /// </summary>
        [NotMapped]
        public abstract List<SbomPackage> Packages { get; }

        /// <summary>
        /// パッケージの依存関係グラフ
        /// </summary>
        [NotMapped]
        public abstract DependencyGraph DependencyGraph { get; }


        /// <summary>
        /// インスタンスを作成します．
        /// </summary>
        /// <remarks>ORMで使用するために用意しています．</remarks>
        public Sbom()
        {
            Name = default!;
            Content = default!;
            Files = default!;
        }

        /// <summary>
        /// 指定したパスのファイル情報からインスタンスを作成します．
        /// </summary>
        /// <param name="name"></param>
        /// <param name="filePath"></param>
        /// <param name="localDirectory"></param>
        /// <remarks>データ新規追加時に呼び出されます．</remarks>
        public Sbom(string name, string filePath, string? localDirectory = null) : this(name, File.ReadAllBytes(filePath), localDirectory) { }

        /// <summary>
        /// 指定したコンテンツからインスタンスを作成します．
        /// </summary>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <param name="localDirectory"></param>
        public Sbom(string name, byte[] content, string? localDirectory = null)
        {
            Name = name;
            Content = content;
            LocalDirectory = localDirectory;
            Files = default!;
        }
    }
}
