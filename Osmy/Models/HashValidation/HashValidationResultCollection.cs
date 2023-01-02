using System;
using System.Collections.Generic;
using System.Linq;

namespace Osmy.Models.HashValidation
{
    public class HashValidationResultCollection
    {
        public int Id { get; set; }
        public DateTime Executed { get; set; }
        public bool HasError { get; set; }
        public Sbom.Sbom Sbom { get; set; }
        public int SbomId { get; set; }
        public List<HashValidationResult> Results { get; set; }

        public HashValidationResultCollection(DateTime executed, Sbom.Sbom sbom, IEnumerable<HashValidationResult> hashValidationResults)
        {
            Executed = executed;
            Sbom = sbom;
            Results = hashValidationResults.ToList();
            HasError = Results.Any(x => x.Result != HashValidity.Valid);
        }

        public HashValidationResultCollection()
        {
            Sbom = default!;
            Results = default!;
        }
    }
}
