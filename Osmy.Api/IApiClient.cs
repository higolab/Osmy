using Osmy.Core.Data.Sbom;
using Osmy.Core.Data.Sbom.ChecksumVerification;

namespace Osmy.Api
{
    public interface IApiClient : IDisposable
    {
        Task<IEnumerable<SbomInfo>> GetSbomsAsync(CancellationToken cancellationToken = default);
        
        IEnumerable<SbomInfo> GetSboms();

        Task<Sbom?> CreateSbomAsync(AddSbomInfo info, CancellationToken cancellationToken = default);

        Task<Sbom?> UpdateSbomAsync(long sbomId, UpdateSbomInfo info, CancellationToken cancellationToken= default);

        Task<bool> DeleteSbomAsync(long sbomId, CancellationToken cancellationToken = default);

        Task<VulnerabilityScanResult?> GetLatestVulnerabilityScanResultAsync(long sbomId, CancellationToken cancellationToken = default);
        
        VulnerabilityScanResult? GetLatestVulnerabilityScanResult(long sbomId);

        Task<ChecksumVerificationResultCollection?> GetLatestChecksumVerificationResultCollectionAsync(long sbomId, CancellationToken cancellationToken= default);
        
        ChecksumVerificationResultCollection? GetLatestChecksumVerificationResultCollection(long sbomId);
    }
}
