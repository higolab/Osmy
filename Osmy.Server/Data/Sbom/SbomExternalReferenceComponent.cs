using System.ComponentModel.DataAnnotations;

namespace Osmy.Server.Data.Sbom
{
    public class SbomExternalReferenceComponent
    {
        public int Id { get; set; }

        public int SbomId { get; set; }

        public Uri Uri { get; set; }

        public SbomExternalReferenceComponent(Uri uri)
        {
            Uri = uri;
        }

        public SbomExternalReferenceComponent() : this(default!) { }
    }
}
