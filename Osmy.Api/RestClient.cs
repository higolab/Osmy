﻿using Osmy.Core.Configuration;
using Osmy.Core.Data.Sbom;
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
            unixSocketPath ??= DefaultServerConfig.UnixSocketPath;
            _client = new RestSharp.RestClient(CreateUnixSocketHttpClient(unixSocketPath), true, options =>
            {
                options.BaseUrl = new Uri("http://localhost");
            });
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public async Task<IEnumerable<Sbom>> GetSbomsAsync(CancellationToken cancellationToken = default)
        {
            var request = new RestRequest("sboms");
            var sboms = await _client.GetAsync<IEnumerable<Sbom>>(request, cancellationToken).ConfigureAwait(false);

            return sboms ?? Enumerable.Empty<Sbom>();
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

        public IEnumerable<Sbom> GetSboms()
        {
            var request = new RestRequest("Sboms");
            var result = _client.Get<IEnumerable<Sbom>>(request);

            return result ?? Enumerable.Empty<Sbom>();
        }

        public Task<Sbom?> GetSbomAsync(long sbomId, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"Sboms/{sbomId}");
            var result = _client.GetAsync<Sbom>(request, cancellationToken);

            return result;
        }

        public Sbom? GetSbom(long sbomId)
        {
            var request = new RestRequest($"Sboms/{sbomId}");
            var result = _client.Get<Sbom>(request);

            return result;
        }

        public async Task<IEnumerable<Sbom>> GetRelatedSbomsAsync(long sbomId, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"Sboms/{sbomId}/related");
            var result = await _client.GetAsync<IEnumerable<Sbom>>(request, cancellationToken);

            return result ?? Enumerable.Empty<Sbom>();
        }

        public IEnumerable<Sbom> GetRelatedSboms(long sbomId)
        {
            var request = new RestRequest($"Sboms/{sbomId}/related");
            var result = _client.Get<IEnumerable<Sbom>>(request);

            return result ?? Enumerable.Empty<Sbom>();
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