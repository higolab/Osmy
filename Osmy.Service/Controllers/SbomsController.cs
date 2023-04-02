using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Osmy.Core.Data.Sbom;
using Osmy.Service.Data;
using Osmy.Service.Data.Sbom;
using Osmy.Service.Data.Sbom.Spdx;

namespace Osmy.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class SbomsController : ControllerBase
    {
        private readonly ILogger<SbomsController> _logger;

        public SbomsController(ILogger<SbomsController> logger)
        {
            _logger = logger;
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
        public async Task<ActionResult<Core.Data.Sbom.Sbom>> Post(AddSbomInfo info)
        {
            // TODO
            // SBOMファイルのデータを送信してもらって，ファイルの解析はこちら側で行う方が良さそう
            // 基本的に同一PC上で動作しているはずなのでURIを送ってもらっても処理できそうだが，ファイルの閲覧権限で問題が起こるかも
            // ファイルのデータを送信してもらう方法 https://qiita.com/mserizawa/items/7f1b9e5077fd3a9d336b

            var sbom = new Spdx(info.Name, info.FileName, info.LocalDirectory);
            using var dbContext = new SoftwareDbContext();
            dbContext.Sboms.Add(sbom);
            await dbContext.SaveChangesAsync();

            var sbomInfo = new SbomInfo(SbomDataConverter.ConvertSbom(sbom), null, null);

            return CreatedAtAction(nameof(Get), new { id = sbom.Id }, sbomInfo);
        }

        [HttpPut]
        public Task<ActionResult<Core.Data.Sbom.Sbom>> Put(Core.Data.Sbom.Sbom sbom)
        {
            throw new NotImplementedException();

            //using var dbContext = new ManagedSoftwareContext();
            //var sbom = dbContext.Sboms
            //    .Include(x => x.Files)
            //    .ThenInclude(x => x.Checksums)
            //    .First(x => x.Id == Sbom.Value.Id);
            //sbom.LocalDirectory = Sbom.Value.LocalDirectory;
            //await dbContext.SaveChangesAsync();
        }

        [HttpDelete("{sbomId}")]
        public async Task<ActionResult> Delete(long sbomId)
        {
            using var dbContext = new SoftwareDbContext();
            var sbom = dbContext.Sboms.FirstOrDefaultAsync(sbom => sbom.Id == sbomId);

            if(sbom is null)
            {
                return NotFound();
            }

            dbContext.Remove(sbom);
            await dbContext.SaveChangesAsync();

            return Ok();
        }
    }
}