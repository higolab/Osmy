using Osmy.Gui.Models.Sbom;

namespace Osmy.Gui.Models.ChecksumVerification
{
    public class ChecksumVerificationResult
    {
        public int Id { get; set; }
        public SbomFile SbomFile { get; set; }
        public int SbomFileId { get; set; }
        public ChecksumCorrectness Result { get; set; }

        public ChecksumVerificationResult(SbomFile sbomFile, ChecksumCorrectness result)
        {
            SbomFile = sbomFile;
            Result = result;
        }

        public ChecksumVerificationResult()
        {
            SbomFile = default!;
        }
    }
}
