using Osmy.Core.Data.Sbom;

namespace Osmy.Api
{
    public interface IApiClient : IDisposable
    {
        Task<IEnumerable<SbomInfo>> GetSbomsAsync(CancellationToken cancellationToken = default);

        IEnumerable<SbomInfo> GetSboms();

        Task<Sbom?> GetSbomAsync(long sbomId, CancellationToken cancellationToken = default);

        Sbom? GetSbom(long sbomId);

        Task<Sbom?> CreateSbomAsync(AddSbomInfo info, CancellationToken cancellationToken = default);

        Task<Sbom?> UpdateSbomAsync(long sbomId, UpdateSbomInfo info, CancellationToken cancellationToken = default);

        Task<bool> DeleteSbomAsync(long sbomId, CancellationToken cancellationToken = default);

        Task<IEnumerable<SbomInfo>> GetRelatedSbomsAsync(long sbomId, CancellationToken cancellationToken = default);

        IEnumerable<SbomInfo> GetRelatedSboms(long sbomId);
    }
}
