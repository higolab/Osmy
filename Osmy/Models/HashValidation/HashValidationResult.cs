using Osmy.Models.Sbom;

namespace Osmy.Models.HashValidation
{
    public class HashValidationResult
    {
        public int Id { get; set; }
        public SbomFile SbomFile { get; set; }
        public int SbomFileId { get; set; }
        public HashValidity Result { get; set; }

        public HashValidationResult(SbomFile sbomFile, HashValidity result)
        {
            SbomFile = sbomFile;
            Result = result;
        }

        public HashValidationResult()
        {
            SbomFile = default!;
        }
    }
}
