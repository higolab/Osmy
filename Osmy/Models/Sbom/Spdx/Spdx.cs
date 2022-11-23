using SpdxModels = CycloneDX.Spdx.Models.v2_2;
using QuickGraph;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ImTools;
using Osmy.Views;
using System;
using System.Reflection.Metadata.Ecma335;

namespace Osmy.Models.Sbom.Spdx
{
    /// <summary>
    /// SPDX
    /// </summary>
    public class Spdx : Sbom
    {
        /// <summary>
        /// SPDX文書の内容
        /// </summary>
        private readonly Lazy<SpdxDocumentContent> _content;

        /// <inheritdoc/>
        public override List<SbomPackage> Packages => _content.Value.Packages.Cast<SbomPackage>().ToList();

        /// <inheritdoc/>
        public override SbomPackage RootPackage => _content.Value.RootPackage;

        /// <inheritdoc/>
        public override DependencyGraph DependencyGraph => _content.Value.DependencyGraph;

        /// <inheritdoc/>
        public override List<SbomFile> Files => _content.Value.Files;

        /// <summary>
        /// 
        /// </summary>
        /// <remarks>ORMで使用するために用意しています．</remarks>
        public Spdx()
        {
            _content = new Lazy<SpdxDocumentContent>(() => new SpdxDocumentContent(new MemoryStream(Content)));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="software"></param>
        /// <param name="path"></param>
        /// <param name="isUsing"></param>
        /// <remarks>新規追加時に呼び出されます．</remarks>
        public Spdx(Software software, string path, bool isUsing = false) : base(software, path, isUsing)
        {
            // 作成時は内容確認を行う可能性が高いので即時に読みこむ
            _content = new Lazy<SpdxDocumentContent>(new SpdxDocumentContent(new MemoryStream(Content)));
            RootPackageVersion = RootPackage.Version;
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
            /// ルートパッケージ
            /// </summary>
            public SbomPackage RootPackage { get; }

            /// <summary>
            /// パッケージの依存関係グラフ
            /// </summary>
            public DependencyGraph DependencyGraph { get; }

            /// <summary>
            /// ファイル情報リスト
            /// </summary>
            public List<SbomFile> Files { get; set; }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="stream"></param>
            /// <param name="leaveOpen"><paramref name="stream"/>をDisposeしない場合はtrue</param>
            public SpdxDocumentContent(Stream stream, bool leaveOpen = false)
            {
                var document = SpdxDeserializer.Deserialize(stream);
                if (!leaveOpen)
                {
                    stream.Dispose();
                }

                var rootPackageId = FindRootPackage(document.SPDXID, document.Relationships);
                Packages = document.Packages
                    .Select(x => new SpdxSoftwarePackage(x.Name, x.VersionInfo, x.SPDXID == rootPackageId, x.SPDXID))
                    .ToList();
                RootPackage = Packages.First(x => x.IsRootPackage);
                DependencyGraph = CreateDependencyGraph(document);
                Files = document.Files?.Select(x => new SbomFile(x.FileName, x.Checksums)).ToList() ?? new List<SbomFile>();
            }

            private static string FindRootPackage(string documentId, List<SpdxModels.Relationship> relationships)
            {
                var describes = relationships
                    .Where(x => x.RelationshipType == SpdxModels.RelationshipType.DESCRIBES)
                    .FindFirst(x => x.SpdxElementId == documentId);

                return describes.RelatedSpdxElement;
            }

            private SbomPackage FindPackageById(string id)
            {
                return Packages.FindFirst(x => x.SpdxRefId == id);
            }

            private DependencyGraph CreateDependencyGraph(SpdxModels.SpdxDocument document)
            {
                var relationships = document.Relationships;
                var graph = new DependencyGraph();

                graph.AddVertexRange(Packages);

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
                            graph.AddVerticesAndEdge(new SEdge<SbomPackage>(pkgA, pkgB)); // TODO 他の関係と区別
                            break;
                    }
                }

                return graph;
            }
        }
    }
}
