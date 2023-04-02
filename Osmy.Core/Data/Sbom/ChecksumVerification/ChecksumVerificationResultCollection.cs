namespace Osmy.Core.Data.Sbom.ChecksumVerification
{
    public class ChecksumVerificationResultCollection
    {
        public int Id { get; set; }
        public DateTime Executed { get; set; }
        public bool HasError { get; set; }
        public Sbom Sbom { get; set; }
        public int SbomId { get; set; }
        public List<ChecksumVerificationResult> Results { get; set; }

        public ChecksumVerificationResultCollection(DateTime executed, Sbom sbom, IEnumerable<ChecksumVerificationResult> checksumVerificationResults)
        {
            Executed = executed;
            Sbom = sbom;
            Results = checksumVerificationResults.ToList();
            HasError = Results.Any(x => x.Result != ChecksumCorrectness.Correct);
        }

        public ChecksumVerificationResultCollection()
        {
            Sbom = default!;
            Results = default!;
        }
    }
}
