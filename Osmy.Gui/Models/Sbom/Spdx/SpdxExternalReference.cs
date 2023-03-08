namespace Osmy.Gui.Models.Sbom.Spdx
{
    internal class SpdxExternalReference : SbomExternalReference
    {
        public string DocumentNamespace { get; set; }

        public SpdxExternalReference()
        {
            DocumentNamespace = string.Empty;
        }

        public SpdxExternalReference(string documentNamespace)
        {
            DocumentNamespace = documentNamespace;
        }
    }
}
