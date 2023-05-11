using Osmy.Core.Configuration;
using Osmy.Core.Data.Sbom;
using Osmy.Core.Data.Sbom.ChecksumVerification;
using RestSharp;
using System.Net.Sockets;

namespace Osmy.Api
{
    public class RestClient : IApiClient
    {
        private readonly RestSharp.RestClient _client;

        public RestClient(Uri baseUrl)
        {
            _client = new RestSharp.RestClient(baseUrl);
        }

        public RestClient(string? unixSocketPath = null)
        {
            unixSocketPath ??= DefaultServerSettings.UnixSocketPath;
            _client = new RestSharp.RestClient(CreateUnixSocketHttpClient(unixSocketPath), true, options =>
            {
                options.BaseUrl = new Uri("http://localhost");
            });
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task<ChecksumVerificationResultCollection?> GetLatestChecksumVerificationResultCollectionAsync(long sbomId, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"ChecksumVerificationResults/latest/{sbomId}");
            var response = await _client.ExecuteGetAsync<ChecksumVerificationResultCollection>(request, cancellationToken);

            return ReturnDataIfSuccessful(response);
        }

        public async Task<VulnerabilityScanResult?> GetLatestVulnerabilityScanResultAsync(long sbomId, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"VulnerabilityScanResults/latest/{sbomId}");
            var response = await _client.ExecuteGetAsync<VulnerabilityScanResult>(request, cancellationToken);

            return ReturnDataIfSuccessful(response);
        }

        public async Task<IEnumerable<SbomInfo>> GetSbomsAsync(CancellationToken cancellationToken = default)
        {
            var request = new RestRequest("sboms");
            var sboms = await _client.GetAsync<IEnumerable<SbomInfo>>(request, cancellationToken).ConfigureAwait(false);

            return sboms ?? Enumerable.Empty<SbomInfo>();
        }

        public async Task<Sbom?> CreateSbomAsync(AddSbomInfo info, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest("Sboms", Method.Post);

            request.AddParameter("name", info.Name);
            request.AddParameter("localDirectory", info.LocalDirectory);
            request.AddFile("file", info.FileName);

            var response = await _client.ExecutePostAsync<Sbom>(request, cancellationToken);

            return ReturnDataIfSuccessful(response);
        }

        public async Task<bool> DeleteSbomAsync(long sbomId, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"Sboms/{sbomId}", Method.Delete);
            var response = await _client.DeleteAsync(request, cancellationToken);

            return response.IsSuccessful;
        }

        public async Task<Sbom?> UpdateSbomAsync(long sbomId, UpdateSbomInfo info, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"Sboms/{sbomId}", Method.Put);
            request.AddBody(info);
            var response = await _client.ExecutePutAsync<Sbom>(request, cancellationToken);

            return ReturnDataIfSuccessful(response);
        }

        public VulnerabilityScanResult? GetLatestVulnerabilityScanResult(long sbomId)
        {
            var request = new RestRequest($"VulnerabilityScanResults/latest/{sbomId}");
            var resopnse = _client.ExecuteGet<VulnerabilityScanResult>(request);

            return ReturnDataIfSuccessful(resopnse);
        }

        public ChecksumVerificationResultCollection? GetLatestChecksumVerificationResultCollection(long sbomId)
        {
            var request = new RestRequest($"ChecksumVerificationResults/latest/{sbomId}");
            var response = _client.ExecuteGet<ChecksumVerificationResultCollection>(request);

            return ReturnDataIfSuccessful(response);
        }

        public IEnumerable<SbomInfo> GetSboms()
        {
            var request = new RestRequest($"Sboms");
            var result = _client.Get<IEnumerable<SbomInfo>>(request);

            return result ?? Enumerable.Empty<SbomInfo>();
        }

        public async Task<IEnumerable<SbomInfo>> GetRelatedSbomsAsync(long sbomId, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"Sboms/{sbomId}/related");
            var result = await _client.GetAsync<IEnumerable<SbomInfo>>(request, cancellationToken);

            return result ?? Enumerable.Empty<SbomInfo>();
        }

        public IEnumerable<SbomInfo> GetRelatedSboms(long sbomId)
        {
            var request = new RestRequest($"Sboms/{sbomId}/related");
            var result = _client.Get<IEnumerable<SbomInfo>>(request);

            return result ?? Enumerable.Empty<SbomInfo>();
        }

        private static HttpClient CreateUnixSocketHttpClient(string socketPath)
        {
            return new HttpClient(new SocketsHttpHandler
            {
                ConnectCallback = async (context, token) =>
                {
                    var socket = new Socket(AddressFamily.Unix, SocketType.Stream, ProtocolType.IP);
                    var endpoint = new UnixDomainSocketEndPoint(socketPath);
                    await socket.ConnectAsync(endpoint);
                    return new NetworkStream(socket, ownsSocket: true);
                }
            });
        }

        private static T? ReturnDataIfSuccessful<T>(RestResponse<T> response)
        {
            return response.IsSuccessful ? response.Data : default;
        }
    }
}