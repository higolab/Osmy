using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Osmy.Core.Data.Sbom.ChecksumVerification;
using Osmy.Service.Data;
using Osmy.Service.Data.Sbom;

namespace Osmy.Service.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class ChecksumVerificationResults : ControllerBase
    {
        [HttpGet("{id}")]
        public Task<ChecksumVerificationResultCollection> Get(long id)
        {
            throw new NotImplementedException();
        }

        [HttpGet("latest/{sbomId}")]
        public async Task<ActionResult<ChecksumVerificationResultCollection>> GetLatest(long sbomId)
        {
            using var dbContext = new SoftwareDbContext();
            var resultCollection = await dbContext.ChecksumVerificationResults
                .Where(x => x.SbomId == sbomId)
                .OrderByDescending(x => x.Executed)
                .Include(x => x.Results)
                .ThenInclude(x => x.SbomFile)
                .ThenInclude(x => x.Checksums)
                .FirstOrDefaultAsync();

            if (resultCollection is null)
            {
                return NotFound();
            }

            foreach (var result in resultCollection.Results)
            {
                result.SbomFile.Checksums = result.SbomFile.Checksums.OrderBy(checksum => checksum.Algorithm).ToList();
            }

            return SbomDataConverter.ConvertChecksumVerificationResultCollection(resultCollection);
        }
    }
}
