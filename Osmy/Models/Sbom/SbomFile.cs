using CycloneDX.Spdx.Models.v2_2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osmy.Models.Sbom
{
    public class SbomFile
    {
        public string FileName { get; set; }

        public List<Checksum> Checksums { get; set; }

        public SbomFile()
        {
            FileName = default!;
            Checksums = default!;
        }

        public SbomFile(string fileName, IEnumerable<Checksum> checksums)
        {
            FileName = fileName;
            Checksums = checksums.ToList();
        }
    }
}
