using OSV.Client;
using OSV.Client.Models;
using OSV.Schema;
using RestSharp;
using System;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Osmy.Models
{
    /// <summary>
    /// パッケージのエコシステムを指定せずに脆弱性のクエリを行える<see cref="IOSVClient"/>の実装です．
    /// </summary>
    internal sealed class OSVClientEx : IOSVClient, IDisposable
    {
        static readonly Version Version = new AssemblyName(typeof(OSVClientEx).Assembly.FullName!).Version!;

        private readonly RestClient _client;

        public OSVClientEx()
        {
            var options = new RestClientOptions("https://api.osv.dev/v1/")
            {
                UserAgent = $"Osmy/{Version}"
            };
            _client = new RestClient(options);
        }

        public void Dispose()
        {
            _client.Dispose();
        }

        public Task<Vulnerability> GetVulnerabilityAsync(string id, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public async Task<VulnerabilityList> QueryAffectedAsync(QueryEx query, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest("query", Method.Post)
            {
                RequestFormat = DataFormat.Json,
            }.AddBody(query);

            return await ExecuteAsync<VulnerabilityList>(request, cancellationToken).ConfigureAwait(false);
        }

        public Task<VulnerabilityList> QueryAffectedAsync(Query query, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        public Task<BatchVulnerabilityList> QueryAffectedBatchAsync(BatchQuery batchQuery, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private async Task<T> ExecuteAsync<T>(RestRequest request, CancellationToken cancellationToken) where T : new()
        {
            try
            {
                var response = await _client.ExecuteAsync<T>(request, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessful)
                {
                    throw new OSVException(null);
                }

                return response.ThrowIfError().Data!;
            }
            catch (Exception ex)
            {
                throw new OSVException(null, ex);
            }
        }
    }

    internal sealed class QueryEx
    {
        [JsonPropertyName("package")]
        public QueryExPackage Package { get; set; }

        [JsonPropertyName("version")]
        public string? Version { get; set; }

        public QueryEx(QueryExPackage package, string? version)
        {
            Package = package;
            Version = version;
        }

        public QueryEx()
        {
            Package = default!;
        }
    }

    internal sealed class QueryExPackage
    {
        [JsonPropertyName("name")]
        public string Name { get; set; }

        [JsonPropertyName("ecosystem")]
        public Ecosystem? Ecosystem { get; set; }

        public QueryExPackage(string name, Ecosystem? ecosystem)
        {
            Name = name;
            Ecosystem = ecosystem;
        }

        public QueryExPackage()
        {
            Name = default!;
        }
    }
}
