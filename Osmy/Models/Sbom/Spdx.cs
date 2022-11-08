using SpdxModels = CycloneDX.Spdx.Models.v2_2;
using QuickGraph;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImTools;
using Osmy.Views;
using System.Text.Json.Serialization;
using System.Text.Json;
using System;

namespace Osmy.Models.Sbom
{
    internal class Spdx : ISbom
    {
        public IPackage RootPackage { get; }

        public IPackage[] Packages => _packages;
        private readonly SpdxSoftwarePackage[] _packages;

        public DependencyGraph DependencyGraph { get; }

        public Spdx(string path)
        {
            var document = Deserialize(path);

            var rootPackageId = FindRootPackage(document.SPDXID, document.Relationships);
            _packages = document.Packages
                .Select(x => new SpdxSoftwarePackage(x.Name, x.VersionInfo, x.SPDXID == rootPackageId, x.SPDXID))
                .ToArray();
            RootPackage = _packages.First(x => x.IsRootPackage);
            DependencyGraph = CreateDependencyGraph(document.Relationships);
        }

        private DependencyGraph CreateDependencyGraph(List<SpdxModels.Relationship> relationships)
        {
            var graph = new DependencyGraph();

            graph.AddVertexRange(_packages);

            foreach (var relationship in relationships)
            {
                var pkgA = FindPackageById(relationship.SpdxElementId);
                var pkgB = FindPackageById(relationship.RelatedSpdxElement);

                switch (relationship.RelationshipType)
                {
                    // A -> B 方向の依存関係
                    case SpdxModels.RelationshipType.CONTAINS:
                    case SpdxModels.RelationshipType.DYNAMIC_LINK:
                    case SpdxModels.RelationshipType.EXPANDED_FROM_ARCHIVE:
                    case SpdxModels.RelationshipType.FILE_ADDED:
                    case SpdxModels.RelationshipType.GENERATED_FROM:
                    case SpdxModels.RelationshipType.PATCH_FOR:
                    case SpdxModels.RelationshipType.STATIC_LINK:
                    case SpdxModels.RelationshipType.HAS_PREREQUISITE:
                    case SpdxModels.RelationshipType.DEPENDS_ON:
                        graph.AddVerticesAndEdge(new SEdge<IPackage>(pkgA, pkgB));
                        break;
                    // B -> A 方向の依存関係
                    case SpdxModels.RelationshipType.CONTAINED_BY:
                    case SpdxModels.RelationshipType.DISTRIBUTION_ARTIFACT:
                    case SpdxModels.RelationshipType.GENERATES:
                    case SpdxModels.RelationshipType.OPTIONAL_COMPONENT_OF:
                    case SpdxModels.RelationshipType.PATCH_APPLIED:
                    case SpdxModels.RelationshipType.PREREQUISITE_FOR:
                    case SpdxModels.RelationshipType.DEPENDENCY_OF:
                    case SpdxModels.RelationshipType.OPTIONAL_DEPENDENCY_OF:
                    case SpdxModels.RelationshipType.RUNTIME_DEPENDENCY_OF:
                        graph.AddVerticesAndEdge(new SEdge<IPackage>(pkgB, pkgA));
                        break;
                    // A <=> B の関係
                    case SpdxModels.RelationshipType.COPY_OF:
                    case SpdxModels.RelationshipType.PACKAGE_OF:
                    case SpdxModels.RelationshipType.VARIANT_OF:
                        graph.AddVerticesAndEdge(new SEdge<IPackage>(pkgA, pkgB)); // TODO 他の関係と区別
                        break;
                }
            }

            return graph;
        }

        private IPackage FindPackageById(string id)
        {
            return _packages.FindFirst(x => x.SpdxRefId == id);
        }

        private static string FindRootPackage(string documentId, List<SpdxModels.Relationship> relationships)
        {
            var describes = relationships
                .Where(x => x.RelationshipType == SpdxModels.RelationshipType.DESCRIBES)
                .FindFirst(x => x.SpdxElementId == documentId);

            return describes.RelatedSpdxElement;
        }

        /// <summary>
        /// SPDX文書を解析します．
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>SPDX文書</returns>
        /// <exception cref="FileFormatException"></exception>
        /// <remarks><see cref="ExternalRefCategoryJsonConverter"/>を用いて解析します．</remarks>
        private static SpdxModels.SpdxDocument Deserialize(string path)
        {
            var options = CycloneDX.Spdx.Serialization.JsonSerializer.GetJsonSerializerOptions_v2_2();
            options.Converters.Insert(0, new ExternalRefCategoryJsonConverter());

            using var stream = File.OpenRead(path);
            return JsonSerializer.Deserialize<SpdxModels.SpdxDocument>(stream, options) ?? throw new FileFormatException();
        }
    }

    record SpdxSoftwarePackage(string Name, string Version, bool IsRootPackage, string SpdxRefId) : IPackage;

    /// <summary>
    /// ケバブケースで記載されたExternalRefsの値を変換するためのコンバーターです．
    /// </summary>
    /// <remarks>ライブラリがSPDX公式のJSONスキーマがPACKAGE_MANAGERとPACKAGE-MANAGERで揺れていた影響を受けているため，このコンバーターによって正しい形式の文書が解析できるように対処します．</remarks>
    public class ExternalRefCategoryJsonConverter : JsonConverter<SpdxModels.ExternalRefCategory>
    {
        public override SpdxModels.ExternalRefCategory Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString()!.Replace("-", "_");
            return (SpdxModels.ExternalRefCategory)Enum.Parse(typeof(SpdxModels.ExternalRefCategory), value);
        }

        public override void Write(Utf8JsonWriter writer, SpdxModels.ExternalRefCategory value, JsonSerializerOptions options)
        {
            var sValue = value.ToString().Replace("_", "-");
            writer.WriteStringValue(sValue);
        }
    }
}
