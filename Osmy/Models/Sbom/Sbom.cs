using Osmy.Views;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.IO;
using System.Linq;

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
        [Key]
        public int Id { get; set; }

        public int SoftwareId { get; set; }
        public virtual Software Software { get; set; }

        /// <summary>
        /// SBOMファイルの内容
        /// </summary>
        public virtual SbomFile SbomFile { get; set; }

        /// <summary>
        /// ルートパッケージのバージョン
        /// </summary>
        public string? RootPackageVersion { get; protected set; }

        /// <summary>
        /// ルートパッケージ
        /// </summary>
        [NotMapped]
        public abstract Package RootPackage { get; }

        /// <summary>
        /// パッケージリスト
        /// </summary>
        [NotMapped]
        public abstract List<Package> Packages { get; }

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
            Software = default!;
            SbomFile = default!;
        }

        /// <summary>
        /// 指定したパスのファイル情報からインスタンスを作成します．
        /// </summary>
        /// <param name="software"></param>
        /// <param name="filePath"></param>
        /// <remarks>データ新規追加時に呼び出されます．</remarks>
        public Sbom(Software software, string filePath)
        {
            Software = software;
            SbomFile = new SbomFile(filePath);
        }
    }

    /// <summary>
    /// SBOMファイル
    /// </summary>
    /// <remarks>このクラスでファイル内容のバイト配列を保持することで，DBから不要なデータが取得されないようにします．</remarks>
    public class SbomFile
    {
        /// <summary>
        /// ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// ファイル内容のバイト配列
        /// </summary>
        public byte[] Data { get; set; }

        /// <summary>
        /// 空のファイルを作成します．
        /// </summary>
        /// <remarks>ORMで使用するために用意しています．</remarks>
        public SbomFile()
        {
            Data = Array.Empty<byte>();
        }

        /// <summary>
        /// 指定したパスのファイルからインスタンスを作成します．
        /// </summary>
        /// <param name="path">SBOMファイルのパス</param>
        public SbomFile(string path)
        {
            using var stream = File.OpenRead(path);
            Data = new byte[stream.Length];
            stream.Read(Data, 0, Data.Length);
        }
    }
}
