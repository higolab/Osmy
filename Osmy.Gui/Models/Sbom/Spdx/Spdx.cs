using SpdxModels = CycloneDX.Spdx.Models.v2_2;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System;
using System.Diagnostics.CodeAnalysis;
using QuikGraph;

namespace Osmy.Gui.Models.Sbom.Spdx
{
    /// <summary>
    /// SPDX
    /// </summary>
    // TODO SbomContent，SpdxContentみたいなクラスを作って上手く処理するように変更する
    // SbomクラスをSBOMの種類ごとに継承するのは止めて，SBOMに共通の情報のみを持つ
    public class Spdx : Sbom
    {
        /// <summary>
        /// SPDX文書の内容
        /// </summary>
        private readonly Lazy<SpdxDocumentContent> _content;

        /// <inheritdoc/>
        public override List<SbomPackage> Packages => _content.Value.Packages.Cast<SbomPackage>().ToList();

        /// <inheritdoc/>
        public override IReadOnlyCollection<SbomPackage> RootPackages => _content.Value.RootPackages;

        /// <inheritdoc/>
        public override DependencyGraph DependencyGraph => _content.Value.DependencyGraph;

        /// <summary>
        /// 名前空間（ドキュメント識別子）
        /// </summary>
        public string DocumentNamespace { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>ORMで使用するために用意しています．</remarks>
        public Spdx()
        {
            _content = new Lazy<SpdxDocumentContent>(() => new SpdxDocumentContent(new MemoryStream(Content)));
            DocumentNamespace = default!;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="name"></param>
        /// <param name="path"></param>
        /// <param name="localDirectory"></param>
        /// <remarks>新規追加時に呼び出されます．</remarks>
        public Spdx(string name, string path, string? localDirectory) : base(name, SpdxConverter.ConvertToJson(path), localDirectory)
        {
            // 作成時は内容確認を行う可能性が高いので即時に読みこむ
            _content = new Lazy<SpdxDocumentContent>(new SpdxDocumentContent(new MemoryStream(Content)));

            using var stream = new MemoryStream(Content);
            var document = SpdxSerializer.Deserialize(stream);
            Files = document.Files?.Select(x => new SbomFile(this, x.FileName, x.Checksums.Select(y => y.ToSbomFileChecksum()))).ToList() ?? new List<SbomFile>();
            DocumentNamespace = document.DocumentNamespace;
            var externalReferences = document.ExternalDocumentRefs?.Select(x => new SpdxExternalReference(x.SpdxDocument)) ?? Enumerable.Empty<SbomExternalReference>();
            ExternalReferences = new List<SbomExternalReference>(externalReferences);
        }

        /// <summary>
        /// SPDX文書の内容
        /// </summary>
        private record SpdxDocumentContent
        {
            /// <summary>
            /// パッケージリスト
            /// </summary>
            public List<SpdxSoftwarePackage> Packages { get; }

            /// <summary>
            /// ルートパッケージリスト
            /// </summary>
            public IReadOnlyCollection<SbomPackage> RootPackages { get; }

            /// <summary>
            /// パッケージの依存関係グラフ
            /// </summary>
            public DependencyGraph DependencyGraph { get; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="leaveOpen"><paramref name="stream"/>をDisposeしない場合はtrue</param>
            public SpdxDocumentContent(Stream stream, bool leaveOpen = false)
            {
                var document = SpdxSerializer.Deserialize(stream);
                if (!leaveOpen)
                {
                    stream.Dispose();
                }

                Packages = document.Packages
                    .Select(x => new SpdxSoftwarePackage(x.Name, x.VersionInfo, document.DocumentDescribes.Contains(x.SPDXID), x.SPDXID))
                    .ToList();
                RootPackages = Packages.Where(x => x.IsRootPackage).ToList().AsReadOnly();
                DependencyGraph = CreateDependencyGraph(document);
            }

            private bool TryFindPackageById(string id, [NotNullWhen(true)] out SbomPackage? package)
            {
                package = Packages.FirstOrDefault(x => x.SpdxRefId == id);
                return package is not null;
            }

            private DependencyGraph CreateDependencyGraph(SpdxModels.SpdxDocument document)
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
                            graph.AddVerticesAndEdge(new SEdge<SbomPackage>(pkgA, pkgB));
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
                            graph.AddVerticesAndEdge(new SEdge<SbomPackage>(pkgB, pkgA));
                            break;
                        // A <=> B の関係
                        case SpdxModels.RelationshipType.COPY_OF:
                        case SpdxModels.RelationshipType.PACKAGE_OF:
                        case SpdxModels.RelationshipType.VARIANT_OF:
                            graph.AddVerticesAndEdge(new SEdge<SbomPackage>(pkgA, pkgB));
                            graph.AddVerticesAndEdge(new SEdge<SbomPackage>(pkgB, pkgA));
                            break;
                    }
                }

                return graph;
            }
        }
    }
}
