using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using Osmy.Core.Data.Sbom;
using Osmy.Server.Data;
using Osmy.Server.Data.Sbom;
using Osmy.Server.Data.Sbom.Spdx;
using Osmy.Server.Services;
using System.Net;

namespace Osmy.Server.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SbomsController : ControllerBase
    {
        private readonly ILogger<SbomsController> _logger;
        private readonly IServiceProvider _serviceProvider;

        public SbomsController(ILogger<SbomsController> logger, IServiceProvider serviceProvider)
        {
            _logger = logger;
            _serviceProvider = serviceProvider;
        }

        [HttpGet]
        public IEnumerable<SbomInfo> Get()
        {
            using var dbContext = new SoftwareDbContext();
            return GetInternal().ToArray();

            IEnumerable<SbomInfo> GetInternal()
            {
                foreach (var sbom in dbContext.Sboms.Include(x => x.ExternalReferences).Include(x => x.Files).ThenInclude(x => x.Checksums))
                {
                    var isVulnerable = dbContext.ScanResults.Where(x => x.SbomId == sbom.Id).AsEnumerable().MaxBy(x => x.Executed)?.IsVulnerable ?? false;
                    var hasFileError = dbContext.ChecksumVerificationResults.Where(x => x.SbomId == sbom.Id).OrderByDescending(x => x.Executed).FirstOrDefault()?.HasError ?? false;
                    yield return new SbomInfo(SbomDataConverter.ConvertSbom(sbom), isVulnerable, hasFileError);
                }
            }
        }

        [HttpPost]
        public async Task<ActionResult<Core.Data.Sbom.Sbom>> Post([FromForm] SbomAddRequest request)
        {
            var sbom = new Spdx(request.Name, request.File, request.LocalDirectory);
            using var dbContext = new SoftwareDbContext();
            dbContext.Sboms.Add(sbom);
            await dbContext.SaveChangesAsync();

            if (sbom.LocalDirectory is not null)
            {
                // �`�F�b�N�T�����؂̎��s�L���[�ɒǉ�
                var checksumService = _serviceProvider.GetRequiredService<ChecksumVerificationService>();
                _ = checksumService.Verify(sbom.Id);
            }

            return CreatedAtAction(nameof(Get), new { id = sbom.Id }, SbomDataConverter.ConvertSbom(sbom));
        }

        [HttpPut("{sbomId}")]
        public async Task<ActionResult<Core.Data.Sbom.Sbom>> Put(long sbomId, UpdateSbomInfo updateSbomInfo)
        {
            using var dbContext = new SoftwareDbContext();
            var sbom = await dbContext.Sboms.FirstOrDefaultAsync(x => x.Id == sbomId);
            if (sbom is null)
            {
                return NotFound();
            }

            if (!string.IsNullOrEmpty(updateSbomInfo.Name))
            {
                sbom.Name = updateSbomInfo.Name;
            }

            if(!string.IsNullOrEmpty(updateSbomInfo.LocalDirectory) && !Directory.Exists(updateSbomInfo.LocalDirectory))
            {
                return BadRequest($"specified directory does not exists.");
            }
            var needVerification = (sbom.LocalDirectory != updateSbomInfo.LocalDirectory) && updateSbomInfo.LocalDirectory is not null;
            sbom.LocalDirectory = updateSbomInfo.LocalDirectory;

            await dbContext.SaveChangesAsync();

            if (needVerification)
            {
                // �`�F�b�N�T�����؂̎��s�L���[�ɒǉ�
                var checksumService = _serviceProvider.GetRequiredService<ChecksumVerificationService>();
                _ = checksumService.Verify(sbom.Id);
            }

            return SbomDataConverter.ConvertSbom(sbom);
        }

        [HttpDelete("{sbomId}")]
        public async Task<ActionResult> Delete(long sbomId)
        {
            using var dbContext = new SoftwareDbContext();
            var sbom = await dbContext.Sboms.FirstOrDefaultAsync(sbom => sbom.Id == sbomId);

            if (sbom is null)
            {
                return NotFound();
            }

            dbContext.Remove(sbom);
            await dbContext.SaveChangesAsync();

            return Ok();
        }

        [HttpGet("{sbomId}/related")]
        public async Task<ActionResult<IEnumerable<SbomInfo>>> GetRelatedSboms(long sbomId)
        {
            using var dbContext = new SoftwareDbContext();
            var queriedSbom = await dbContext.Sboms.Include(x => x.ExternalReferences).FirstOrDefaultAsync(x => x.Id == sbomId);
            return queriedSbom switch
            {
                Spdx => dbContext.Sboms
                .OfType<Spdx>()
                .AsEnumerable()
                .Where(sbom => queriedSbom.ExternalReferences.OfType<SpdxExternalReference>()
                                                             .Any(exref => exref.DocumentNamespace == sbom.DocumentNamespace))
                .Select(sbom =>
                {
                    var isVulnerable = dbContext.ScanResults.Where(result => result.SbomId == sbom.Id)
                                                            .OrderByDescending(x => x.Executed)
                                                            .FirstOrDefault()?.IsVulnerable;
                    var hasFileError = dbContext.ChecksumVerificationResults.Where(result => result.SbomId == sbom.Id)
                                                                            .OrderByDescending(x => x.Executed)
                                                                            .FirstOrDefault()?.HasError;
                    return new SbomInfo(SbomDataConverter.ConvertSbom(sbom), isVulnerable, hasFileError);
                })
                .ToArray(),
                _ => throw new NotSupportedException(),
            };
        }
    }

    public record SbomAddRequest(string Name, string? LocalDirectory, IFormFile File);
}