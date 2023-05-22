namespace Osmy.Core.Util
{
    public static class SpdxUtil
    {
        public static readonly string[] ValidFileExtensions = new[]
        {
            ".json",
            ".rdf.xml",
            ".rdf",
            ".xml",
            ".xls",
            ".xlsx",
            ".yaml",
            ".yml",
            ".tag",
            ".spdx",
            ".rdf.ttl"
        };

        /// <summary>
        /// SPDXドキュメントファイルとして適切な拡張子を持つかを判定します．
        /// </summary>
        /// <param name="fileName">ファイル名</param>
        /// <returns>適切な拡張子を持つ場合true，それ以外はfalse</returns>
        public static bool HasValidExtension(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            if (extension is null) { return false; }

            return extension switch
            {
                ".json" or ".rdf" or ".xml" or ".xls" or ".xlsx" or ".yaml" or ".yml" or ".tag" or ".spdx" => true,
                ".ttl" => fileName.EndsWith(".rdf.ttl"),
                _ => false,
            };
        }
    }
}
