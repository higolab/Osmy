using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Osmy.Models.Sbom
{
    public class HashValidationResult
    {
        public int Id { get; set; }
        public DateTime Executed { get; set; }
        public SbomFile SbomFile { get; set; }
        public bool IsValid { get; set; }

        public HashValidationResult(DateTime executed, SbomFile sbomFile, bool isValid)
        {
            Executed = executed;
            SbomFile = sbomFile;
            IsValid = isValid;
        }

        public HashValidationResult()
        {
            SbomFile = default!;
        }
    }
}
