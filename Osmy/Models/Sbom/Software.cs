using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osmy.Models.Sbom
{
    /// <summary>
    /// ソフトウェア情報
    /// </summary>
    public class Software
    {
        /// <summary>
        /// ID
        /// </summary>
        [Key]
        public int Id { get; set; }

        /// <summary>
        /// 名前
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// このソフトウェアのSBOMリスト
        /// </summary>
        public List<Sbom> Sboms { get; set; }

        /// <summary>
        /// 使用しているSBOM
        /// </summary>
        public Sbom UsingSbom => Sboms.First(x => x.IsUsing);

        /// <summary>
        /// ソフトウェアのディスク上の位置
        /// </summary>
        public string? LocalDirectory { get; set; }

        /// <summary>
        /// 空のインスタンスを作成します．
        /// </summary>
        /// <remarks>ORMで使用するために用意しています．</remarks>
        public Software()
        {
            Name = default!;
            Sboms = default!;
        }

        /// <summary>
        /// 名前とSBOMファイルのパスを指定してインスタンスを作成します．
        /// </summary>
        /// <param name="name">名前</param>
        /// <param name="sbomFile">SBOMファイルのパス</param>
        public Software(string name, string sbomFile)
        {
            Name = name;
            var sbom = new Spdx.Spdx(this, sbomFile, true);
            Sboms = new List<Sbom>() { sbom };
            sbom.IsUsing = true;
        }

        // TODO
        public event Action? VulnerabilityScanned;

        public void RaiseVulnerabilityScanned()
        {
            VulnerabilityScanned?.Invoke();
        }
    }
}
