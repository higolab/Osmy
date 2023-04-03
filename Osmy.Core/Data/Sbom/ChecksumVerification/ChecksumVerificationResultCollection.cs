namespace Osmy.Core.Data.Sbom.ChecksumVerification
{
    public class ChecksumVerificationResultCollection
    {
        public long Id { get; set; }
        public DateTime Executed { get; set; }
        public bool HasError { get; set; }
        public long SbomId { get; set; }
        public IEnumerable<ChecksumVerificationResult> Results { get; set; }

        public ChecksumVerificationResultCollection(long id,　DateTime executed, long sbomId, IEnumerable<ChecksumVerificationResult> checksumVerificationResults)
        {
            Id = id;
            Executed = executed;
            SbomId = sbomId;
            Results = checksumVerificationResults.ToList();
            HasError = Results.Any(x => x.Result != ChecksumCorrectness.Correct);
        }

        public ChecksumVerificationResultCollection()
        {
            Results = Enumerable.Empty<ChecksumVerificationResult>();
        }
    }
}
