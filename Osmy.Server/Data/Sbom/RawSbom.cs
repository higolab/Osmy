namespace Osmy.Server.Data.Sbom
{
    public class RawSbom
    {
        public long Id { get; set; }

        public long SbomId { get; set; }

        public byte[] Data { get; set; }

        public RawSbom() : this(Array.Empty<byte>()) { }

        public RawSbom(byte[] data)
        {
            Data = data;
        }
    }
}
