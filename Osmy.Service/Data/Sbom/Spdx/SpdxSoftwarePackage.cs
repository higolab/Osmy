using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osmy.Service.Data.Sbom.Spdx
{
    /// <summary>
    /// SPDXに記載されたパッケージ情報
    /// </summary>
    public class SpdxSoftwarePackage : SbomPackage
    {
        public string SpdxRefId { get; set; }

        public SpdxSoftwarePackage()
        {
            SpdxRefId = string.Empty;
        }

        public SpdxSoftwarePackage(string name, string version, bool isRootPackage, string spdxRefId)
        {
            Name = name;
            Version = version;
            IsRootPackage = isRootPackage;
            SpdxRefId = spdxRefId;
        }
    }
}
