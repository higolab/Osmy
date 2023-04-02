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

        public RestClient(string unixSocketPath)
        {
            _client = new RestSharp.RestClient(CreateUnixSocketHttpClient(unixSocketPath), true, options =>
            {
                options.BaseUrl = new Uri("http://localhost");
            });
        }

#if DEBUG
        public RestClient() : this(Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData), "Osmy", "osmy.service.sock")) { }
#endif

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task<ChecksumVerificationResultCollection?> GetLatestChecksumVerificationResultCollectionAsync(long sbomId, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"ChecksumVerificationResults/latest/{sbomId}");
            var result = await _client.GetAsync<ChecksumVerificationResultCollection>(request, cancellationToken);

            return result;
        }

        public async Task<VulnerabilityScanResult?> GetLatestVulnerabilityScanResultAsync(long sbomId, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"VulnerabilityScanResults/latest/{sbomId}");
            var result = await _client.GetAsync<VulnerabilityScanResult>(request, cancellationToken);

            return result;
        }

        public async Task<IEnumerable<SbomInfo>> GetSbomsAsync(CancellationToken cancellationToken = default)
        {
            var request = new RestRequest("sboms");
            var sboms = await _client.GetAsync<IEnumerable<SbomInfo>>(request, cancellationToken).ConfigureAwait(false);

            return sboms ?? Enumerable.Empty<SbomInfo>();
        }

        public async Task<SbomInfo?> CreateSbomAsync(AddSbomInfo info, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest("Sboms", Method.Post);
            request.AddBody(info);

            var result = await _client.PostAsync<SbomInfo>(request, cancellationToken);

            return result;
        }

        public Task DeleteSbomAsync(long sbomId, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task UpdateSbomAsync(Sbom sbom, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public VulnerabilityScanResult? GetLatestVulnerabilityScanResult(long sbomId)
        {
            var request = new RestRequest($"VulnerabilityScanResults/latest/{sbomId}");
            var result = _client.Get<VulnerabilityScanResult>(request);

            return result;
        }

        public ChecksumVerificationResultCollection? GetLatestChecksumVerificationResultCollection(long sbomId)
        {
            var request = new RestRequest($"ChecksumVerificationResults/latest/{sbomId}");
            var result = _client.Get<ChecksumVerificationResultCollection>(request);

            return result;
        }

        public IEnumerable<SbomInfo> GetSboms()
        {
            var request = new RestRequest($"Sboms");
            var result = _client.Get<IEnumerable<SbomInfo>>(request);

            return result ?? Enumerable.Empty<SbomInfo>();
        }

        private HttpClient CreateUnixSocketHttpClient(string socketPath)
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
    }
}