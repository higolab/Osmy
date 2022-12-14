using System;

namespace Osmy.Models.Sbom
{
    public class HashValidation
    {
        public int Id { get; set; }
        public DateTime Executed { get; set; }
        public SbomFile SbomFile { get; set; }
        public int SbomFileId { get; set; }
        public HashValidationResult Result { get; set; }

        public HashValidation(DateTime executed, SbomFile sbomFile, HashValidationResult result)
        {
            Executed = executed;
            SbomFile = sbomFile;
            Result = result;
        }

        public HashValidation()
        {
            SbomFile = default!;
        }
    }

    public enum HashValidationResult
    {
        Invalid,
        Valid,
        FileNotFound,
    }
}
