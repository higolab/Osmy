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
            // SBOM�t�@�C���̃f�[�^�𑗐M���Ă�����āC�t�@�C���̉�͂͂����瑤�ōs�������ǂ�����
            // ��{�I�ɓ���PC��œ��삵�Ă���͂��Ȃ̂�URI�𑗂��Ă�����Ă������ł����������C�t�@�C���̉{�������Ŗ�肪�N���邩��
            // �t�@�C���̃f�[�^�𑗐M���Ă��炤���@ https://qiita.com/mserizawa/items/7f1b9e5077fd3a9d336b

            var sbom = new Spdx(info.Name, info.FileName, info.LocalDirectory);
            using var dbContext = new SoftwareDbContext();
            dbContext.Sboms.Add(sbom);
            await dbContext.SaveChangesAsync();

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
            sbom.LocalDirectory = updateSbomInfo.LocalDirectory;

            await dbContext.SaveChangesAsync();

            // TODO �`�F�b�N�T�����؂̎��s�҂����X�g�ɓo�^�H

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
    }
}