using Osmy.Core.Data.Sbom;
using Osmy.Server.Data.Sbom;

namespace Osmy.Server.Tests.Data.Sbom
{
    public class SbomDataConverterTest
    {
        [Theory]
        [MemberData(nameof(CreateConvertSbomTestData))]
        public void ConvertSbom_ShouldReturnConvertedData(Server.Data.Sbom.Sbom sbom)
        {
            var converted = SbomDataConverter.ConvertSbom(sbom);

            Assert.Equal(sbom.Id, converted.Id);
            Assert.Equal(sbom.Name, converted.Name);
            Assert.Equal(sbom.LocalDirectory, converted.LocalDirectory);
            Assert.Equal(sbom.LastVulnerabilityScan, converted.LastVulnerabilityScan);
            Assert.Equal(sbom.IsVulnerable, converted.IsVulnerable);
            Assert.Equal(sbom.LastFileCheck, converted.LastFileCheck);
            Assert.Equal(sbom.HasFileError, converted.HasFileError);

            // TODO リストになっている項目の一致確認
        }

        [Theory]
        [MemberData(nameof(CreateConvertSbomPackageTestData))]
        public void ConvertSbomPackage_ShouldReturnConvertedData(SbomPackageComponent package)
        {
            var converted = SbomDataConverter.ConvertSbomPackage(package);

            Assert.Equal(package.Id, converted.Id);
            Assert.Equal(package.Name, converted.Name);
            Assert.Equal(package.IsRootPackage, converted.IsRootPackage);
            Assert.Equal(package.IsDependentPackage, converted.IsDependentPackage);

            // TODO リストになっている項目の一致確認
        }

        [Theory]
        [MemberData(nameof(CreateConvertSbomFileTestData))]
        public void ConvertSbomFile_ShouldReturnConvertedData(SbomFileComponent sbomFile)
        {
            var converted = SbomDataConverter.ConvertSbomFile(sbomFile);

            Assert.Equal(sbomFile.Id, converted.Id);
            Assert.Equal(sbomFile.FileName, converted.FileName);
            Assert.Equal(sbomFile.Status, converted.Status);

            // TODO リストになっている項目の一致確認
        }

        [Theory]
        [MemberData(nameof(CreateConvertExternalReferenceTestData))]
        public void ConvertExternalReference(SbomExternalReferenceComponent externalReference)
        {
            var converted = SbomDataConverter.ConvertExternalReference(externalReference);

            Assert.Equal(externalReference.SbomId, converted.SbomId);
        }

        [Theory]
        [MemberData(nameof(CreateConvertVulnerabilityDataTestData))]
        public void ConvertVulnerabilityData(Server.Data.Sbom.VulnerabilityData vulnerabilityData)
        {
            var converted = SbomDataConverter.ConvertVulnerabilityData(vulnerabilityData);

            Assert.Equal(vulnerabilityData.Id, converted.Id);
            Assert.Equal(vulnerabilityData.Modified, converted.Modified);
            Assert.Equal(vulnerabilityData.Data.Data, converted.Data);
        }

        public static IEnumerable<object[]> CreateConvertSbomTestData()
        {
            yield return new object[]
            {
                new Server.Data.Sbom.Sbom
                {
                    Id = 1,
                    Name = "Test",
                    LocalDirectory = @"C:\",
                    LastVulnerabilityScan = DateTime.MinValue,
                    IsVulnerable = true,
                    LastFileCheck = DateTime.MaxValue,
                    HasFileError = true,
                }
            };
        }

        public static IEnumerable<object[]> CreateConvertSbomPackageTestData()
        {
            yield return new object[]
            {
                new SbomPackageComponent
                {
                    Id = 1,
                    SbomId = 1,
                    Name = "Test",
                    IsRootPackage = true,
                    IsDependentPackage = true,
                }
            };
        }

        public static IEnumerable<object[]> CreateConvertSbomFileTestData()
        {
            yield return new object[]
            {
                new SbomFileComponent
                {
                    Id = 1,
                    SbomId = 1,
                    FileName = "Test",
                    Status = ChecksumCorrectness.Correct,
                }
            };
        }

        public static IEnumerable<object[]> CreateConvertExternalReferenceTestData()
        {
            yield return new object[]
            {
                new SbomExternalReferenceComponent
                {
                    Id = 1,
                    SbomId = 1,
                    Uri = new Uri("https://example.com/")
                }
            };
        }

        public static IEnumerable<object[]> CreateConvertVulnerabilityDataTestData()
        {
            yield return new object[]
            {
                new Server.Data.Sbom.VulnerabilityData
                {
                    Id = "Test",
                    Modified = DateTimeOffset.MinValue,
                    Data = new RawVulnerabilityData("{}"),
                }
            };
        }
    }
}