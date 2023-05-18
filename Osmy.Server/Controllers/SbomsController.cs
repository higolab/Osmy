using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Osmy.Core.Data.Sbom;
using Osmy.Server.Data;
using Osmy.Server.Data.Sbom;
using Osmy.Server.Services;

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
        public IEnumerable<Core.Data.Sbom.Sbom> Get()
        {
            using var dbContext = new SoftwareDbContext();
            return GetInternal().ToArray();

            IEnumerable<Core.Data.Sbom.Sbom> GetInternal()
            {
                foreach (var sbom in dbContext.Sboms.Include(x => x.ExternalReferences))
                {
                    /* 
                     * #17のワークアラウンド
                     * Sbom.Contentが複製されてメモリ消費量が大きくなるのを防ぐため，ファイルとチェックサムはSBOM情報と分けて取得する
                     */
                    var files = dbContext.Files.Where(x => x.SbomId == sbom.Id).Include(x => x.Checksums).ToList();
                    sbom.Files = files;

                    yield return SbomDataConverter.ConvertSbom(sbom);
                }
            }
        }

        [HttpGet("{sbomId}")]
        public async Task<ActionResult<Core.Data.Sbom.Sbom>> Get(long sbomId)
        {
            using var dbContext = new SoftwareDbContext();
            var sbom = await dbContext.Sboms.Include(x => x.Packages)
                                            .ThenInclude(x => x.Vulnerabilities)
                                            .Include(x => x.Files)
                                            .Include(x => x.ExternalReferences)
                                            .FirstOrDefaultAsync(x => x.Id == sbomId);
            if (sbom is null)
            {
                return NotFound();
            }

            return SbomDataConverter.ConvertSbom(sbom);
        }

        [HttpPost]
        public async Task<ActionResult<Core.Data.Sbom.Sbom>> Post([FromForm] SbomAddRequest request)
        {
            var sbom = new Data.Sbom.Sbom(request.Name, request.File, request.LocalDirectory);
            using var dbContext = new SoftwareDbContext();
            dbContext.Sboms.Add(sbom);
            await dbContext.SaveChangesAsync();

            if (sbom.LocalDirectory is not null)
            {
                // 脆弱性診断のキューに追加
                var vulnerabilityScanService = _serviceProvider.GetRequiredService<VulnerabilityScanService>();
                _ = VulnerabilityScanService.ScanAsync(sbom.Id);

                // チェックサム検証の実行キューに追加
                var checksumService = _serviceProvider.GetRequiredService<ChecksumVerificationService>();
                _ = checksumService.VerifyAsync(sbom.Id);
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

            if (!string.IsNullOrEmpty(updateSbomInfo.LocalDirectory) && !Directory.Exists(updateSbomInfo.LocalDirectory))
            {
                return BadRequest($"specified directory does not exists.");
            }
            var needVerification = (sbom.LocalDirectory != updateSbomInfo.LocalDirectory) && updateSbomInfo.LocalDirectory is not null;
            sbom.LocalDirectory = updateSbomInfo.LocalDirectory;

            await dbContext.SaveChangesAsync();

            if (needVerification)
            {
                // チェックサム検証の実行キューに追加
                var checksumService = _serviceProvider.GetRequiredService<ChecksumVerificationService>();
                _ = checksumService.VerifyAsync(sbom.Id);
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
        public async Task<ActionResult<IEnumerable<Core.Data.Sbom.Sbom>>> GetRelatedSboms(long sbomId)
        {
            using var dbContext = new SoftwareDbContext();
            var queriedSbom = await dbContext.Sboms.Include(x => x.ExternalReferences).FirstOrDefaultAsync(x => x.Id == sbomId);

            return queriedSbom switch
            {
                Data.Sbom.Sbom => dbContext.ExternalReferences
                .Where(x => x.SbomId == sbomId)
                .Join(dbContext.Sboms, externalRef => externalRef.Uri, sbom => sbom.Uri, (externalRef, sbom) => sbom)
                .AsEnumerable()
                .Select(SbomDataConverter.ConvertSbom)
                .ToArray(),
                _ => throw new NotSupportedException(),
            };
        }
    }

    public record SbomAddRequest(string Name, string? LocalDirectory, IFormFile File);
}