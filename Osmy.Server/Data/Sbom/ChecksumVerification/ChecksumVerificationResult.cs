using Osmy.Core.Data.Sbom.ChecksumVerification;
using Osmy.Server.Data.Sbom;

namespace Osmy.Server.Data.ChecksumVerification
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
