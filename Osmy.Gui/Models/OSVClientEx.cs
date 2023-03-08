using OSV.Client;
using OSV.Client.Models;
using OSV.Schema;
using RestSharp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Osmy.Gui.Models
{
    /// <summary>
    /// パッケージのエコシステムを指定せずに脆弱性のクエリを行えるOSV APIのクライアントです．
    /// </summary>
    /// <seealso cref="OSVClient"/>
    internal sealed class OSVClientEx : IDisposable
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

        public async Task<VulnerabilityList> QueryAffectedAsync(QueryEx query, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest("query", Method.Post)
            {
                RequestFormat = DataFormat.Json,
            }.AddBody(query);

            return await ExecuteAsync<VulnerabilityList>(request, cancellationToken).ConfigureAwait(false);
        }

        public async Task<BatchVulnerabilityList> QueryAffectedBatchAsync(BatchQueryEx batchQuery, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest("querybatch", Method.Post)
            {
                RequestFormat = DataFormat.Json,
            }.AddBody(batchQuery);

            var batchResult = await ExecuteAsync<BatchVulnerabilityList>(request, cancellationToken).ConfigureAwait(false);

            // バッチクエリでは脆弱性のIDと情報変更日しか返されないので全データを取得
            foreach (var result in batchResult.Results)
            {
                if (result.Vulnerabilities is null) { continue; }
                int vulnCount = result.Vulnerabilities.Count();
                if (vulnCount == 0) { continue; }

                var vulns = new Vulnerability[vulnCount];
                int index = 0;
                foreach (var vuln in result.Vulnerabilities)
                {
                    vulns[index++] = await GetVulnerabilityById(vuln.Id, cancellationToken).ConfigureAwait(false);
                }

                result.Vulnerabilities = vulns;
            }

            return batchResult;
        }

        public async Task<Vulnerability> GetVulnerabilityById(string id, CancellationToken cancellationToken = default)
        {
            var request = new RestRequest($"vulns/{id}");
            return await ExecuteAsync<Vulnerability>(request, cancellationToken).ConfigureAwait(false);
        }

        private async Task<T> ExecuteAsync<T>(RestRequest request, CancellationToken cancellationToken) where T : new()
        {
            try
            {
                var response = await _client.ExecuteAsync<T>(request, cancellationToken).ConfigureAwait(false);
                if (!response.IsSuccessful)
                {
                    throw new OSVException(response.ErrorMessage, response.ErrorException);
                }

                return response.ThrowIfError().Data!;
            }
            catch (Exception ex) when (ex is not OSVException)
            {
                throw new OSVException(ex.Message, ex);
            }
        }
    }

    internal sealed record QueryEx
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

    internal sealed record QueryExPackage
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

    internal sealed class BatchQueryEx
    {
        [JsonPropertyName("queries")]
        public QueryEx[] Queries { get; set; }

        public BatchQueryEx(IEnumerable<QueryEx> queries)
        {
            if (queries is QueryEx[] array)
            {
                Queries = array;
            }
            else
            {
                Queries = queries.ToArray();
            }
        }
    }
}
