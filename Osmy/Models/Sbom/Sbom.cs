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

        public Software Software { get; set; }

        /// <summary>
        /// 現在使用中か
        /// </summary>
        public bool IsUsing { get; set; }

        /// <summary>
        /// ファイル内容のバイト配列
        /// </summary>
        public byte[] Content { get; set; }

        /// <summary>
        /// SBOMファイルのハッシュ値
        /// </summary>
        public byte[] ContentHash { get; set; }

        /// <summary>
        /// ルートパッケージのバージョン
        /// </summary>
        public string? RootPackageVersion { get; protected set; }
        
        /// <summary>
        /// ファイルリスト
        /// </summary>
        public List<SbomFile> Files { get; set; }

        /// <summary>
        /// ルートパッケージ
        /// </summary>
        [NotMapped]
        public abstract SbomPackage RootPackage { get; }

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
            Software = default!;
            Content = default!;
            ContentHash = default!;
        }

        /// <summary>
        /// 指定したパスのファイル情報からインスタンスを作成します．
        /// </summary>
        /// <param name="software"></param>
        /// <param name="filePath"></param>
        /// <param name="isUsing"></param>
        /// <remarks>データ新規追加時に呼び出されます．</remarks>
        public Sbom(Software software, string filePath, bool isUsing = false)
        {
            Software = software;
            IsUsing = isUsing;
            Content = File.ReadAllBytes(filePath);
            ContentHash = ComputeHash();
        }

        public byte[] ComputeHash()
        {
            return ComputeHash(Content);
        }

        public static async Task<byte[]> ComputeHashAsync(string path)
        {
            if (!File.Exists(path))
            {
                throw new FileNotFoundException();
            }

            using var stream = File.OpenRead(path);
            return await ComputeHashAsync(stream);
        }

        public static async Task<byte[]> ComputeHashAsync(Stream stream)
        {
            var provider = HashAlgorithm.Create("MD5")!;
            return await provider.ComputeHashAsync(stream);
        }

        public static byte[] ComputeHash(byte[] buffer)
        {
            var provider = HashAlgorithm.Create("MD5")!;
            return provider.ComputeHash(buffer);
        }
    }
}
