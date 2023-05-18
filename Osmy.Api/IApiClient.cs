using Osmy.Core.Data.Sbom;

namespace Osmy.Api
{
    public interface IApiClient : IDisposable
    {
        Task<IEnumerable<Sbom>> GetSbomsAsync(CancellationToken cancellationToken = default);

        IEnumerable<Sbom> GetSboms();

        Task<Sbom?> GetSbomAsync(long sbomId, CancellationToken cancellationToken = default);

        Sbom? GetSbom(long sbomId);

        Task<Sbom?> CreateSbomAsync(AddSbomInfo info, CancellationToken cancellationToken = default);

        Task<Sbom?> UpdateSbomAsync(long sbomId, UpdateSbomInfo info, CancellationToken cancellationToken = default);

        Task<bool> DeleteSbomAsync(long sbomId, CancellationToken cancellationToken = default);

        Task<IEnumerable<Sbom>> GetRelatedSbomsAsync(long sbomId, CancellationToken cancellationToken = default);

        IEnumerable<Sbom> GetRelatedSboms(long sbomId);
    }
}
