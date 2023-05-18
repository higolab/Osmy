using SpdxModels = CycloneDX.Spdx.Models.v2_2;
using System.Diagnostics.CodeAnalysis;
using QuikGraph;
using QuikGraph.Algorithms.Search;
using CycloneDX.Spdx.Models.v2_2;

namespace Osmy.Server.Data.Sbom.Spdx
{
    /// <summary>
    /// SPDXドキュメントの解析ヘルパー
    /// </summary>
    public class SpdxDocumentParseHelper
    {
        private readonly SpdxDocument _document;

        public Uri Uri => new(_document.DocumentNamespace);

        /// <summary>
        /// パッケージリスト
        /// </summary>
        public List<SbomPackageComponent> Packages { get; }

        /// <summary>
        /// ルートパッケージリスト
        /// </summary>
        public IReadOnlyCollection<SbomPackageComponent> RootPackages { get; }

        /// <summary>
        /// ファイルリスト
        /// </summary>
        public List<SbomFileComponent> Files { get; set; }

        /// <summary>
        /// 外部参照
        /// </summary>
        public List<SbomExternalReferenceComponent> ExternalReferences { get; set; }

        /// <summary>
        /// パッケージの依存関係グラフ
        /// </summary>
        public DependencyGraph DependencyGraph { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="leaveOpen"><paramref name="stream"/>をDisposeしない場合はtrue</param>
        public SpdxDocumentParseHelper(Sbom sbom, Stream stream, bool leaveOpen = false)
        {
            _document = SpdxSerializer.Deserialize(stream);
            if (!leaveOpen)
            {
                stream.Dispose();
            }

            var describedPackageIds = _document.Relationships
                .Where(x => x.SpdxElementId == _document.SPDXID && x.RelationshipType == SpdxModels.RelationshipType.DESCRIBES)
                .Select(x => x.RelatedSpdxElement)
                .Concat(_document.DocumentDescribes ?? Enumerable.Empty<string>())
                .Distinct()
                .ToArray();

            Packages = _document.Packages
                .Select(CreateSbomPackageComponent(describedPackageIds))
                .ToList();
            RootPackages = Packages.Where(x => x.IsRootPackage).ToList().AsReadOnly();

            Files = _document.Files?.Select(CreateSbomFileComponent(sbom)).ToList() ?? new List<SbomFileComponent>();

            ExternalReferences = _document.ExternalDocumentRefs?.Select(CreateSbomExternalReference()).ToList()
                ?? new List<SbomExternalReferenceComponent>();

            DependencyGraph = CreateDependencyGraph(_document);
            // MEMO: dfs.VisitedGraphはコンストラクタで渡したグラフのことで，実際に訪問した頂点と辺からなるグラフではない
            var dfs = new DepthFirstSearchAlgorithm<SbomPackageComponent, IEdge<SbomPackageComponent>>(DependencyGraph);
            dfs.TreeEdge += x => x.Target.IsDependentPackage = true;
            foreach (SbomPackageComponent package in RootPackages)
            {
                dfs.Compute(package);
            }
        }

        private static Func<ExternalDocumentRef, SbomExternalReferenceComponent> CreateSbomExternalReference()
        {
            return x => new SbomExternalReferenceComponent(new Uri(x.SpdxDocument));
        }

        private static Func<SpdxModels.File, SbomFileComponent> CreateSbomFileComponent(Sbom sbom)
        {
            return x => new SbomFileComponent(sbom, x.FileName, x.Checksums.Select(y => y.ToSbomFileChecksum()));
        }

        private static Func<Package, SbomPackageComponent> CreateSbomPackageComponent(string[] describedPackageIds)
        {
            return x => new SbomPackageComponent(x.Name, x.SPDXID, x.VersionInfo, describedPackageIds.Contains(x.SPDXID));
        }

        private bool TryFindPackageById(string id, [NotNullWhen(true)] out SbomPackageComponent? package)
        {
            package = Packages.FirstOrDefault(x => x.ReferenceId == id);
            return package is not null;
        }

        private DependencyGraph CreateDependencyGraph(SpdxDocument document)
        {
            var relationships = document.Relationships;
            var graph = new DependencyGraph();

            graph.AddVertexRange(Packages);

            foreach (var relationship in relationships)
            {
                if (!TryFindPackageById(relationship.SpdxElementId, out var pkgA))
                {
                    continue;
                }
                if (!TryFindPackageById(relationship.RelatedSpdxElement, out var pkgB))
                {
                    continue;
                }

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
                        graph.AddVerticesAndEdge(new SEdge<SbomPackageComponent>(pkgA, pkgB));
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
                        graph.AddVerticesAndEdge(new SEdge<SbomPackageComponent>(pkgB, pkgA));
                        break;
                    // A <=> B の関係
                    case SpdxModels.RelationshipType.COPY_OF:
                    case SpdxModels.RelationshipType.PACKAGE_OF:
                    case SpdxModels.RelationshipType.VARIANT_OF:
                        graph.AddVerticesAndEdge(new SEdge<SbomPackageComponent>(pkgA, pkgB));
                        graph.AddVerticesAndEdge(new SEdge<SbomPackageComponent>(pkgB, pkgA));
                        break;
                }
            }

            return graph;
        }
    }
}
