using SpdxModels = CycloneDX.Spdx.Models.v2_2;
using System.Text.Json.Serialization;
using System.Text.Json;

namespace Osmy.Service.Data.Sbom.Spdx
{
    internal class SpdxSerializer
    {
        /// <summary>
        /// SPDX文書を解析します．
        /// </summary>
        /// <param name="path">ファイルパス</param>
        /// <returns>SPDX文書</returns>
        /// <exception cref="FileFormatException"></exception>
        /// <remarks><see cref="ExternalRefCategoryJsonConverter"/>を用いて解析します．</remarks>
        public static SpdxModels.SpdxDocument Deserialize(string path)
        {
            using var stream = File.OpenRead(path);
            return Deserialize(stream);
        }

        /// <summary>
        /// SPDX文書を解析します．
        /// </summary>
        /// <param name="stream">内容のストリーム</param>
        /// <returns>SPDX文書</returns>
        /// <exception cref="FileFormatException"></exception>
        /// <remarks><see cref="ExternalRefCategoryJsonConverter"/>を用いて解析します．</remarks>
        public static SpdxModels.SpdxDocument Deserialize(Stream stream)
        {
            var options = CycloneDX.Spdx.Serialization.JsonSerializer.GetJsonSerializerOptions_v2_2();
            options.Converters.Insert(0, new ExternalRefCategoryJsonConverter());

            return JsonSerializer.Deserialize<SpdxModels.SpdxDocument>(stream, options) ?? throw new IOException(); // TODO 適切な例外の使用
        }

        public static void Serialize(SpdxModels.SpdxDocument document, string path)
        {
            using var stream = File.OpenWrite(path);
            Serialize(document, stream);
        }

        public static void Serialize(SpdxModels.SpdxDocument document, Stream stream)
        {
            var options = CycloneDX.Spdx.Serialization.JsonSerializer.GetJsonSerializerOptions_v2_2();
            options.Converters.Insert(0, new ExternalRefCategoryJsonConverter());

            JsonSerializer.Serialize<SpdxModels.SpdxDocument>(stream, document, options);
        }
    }

    /// <summary>
    /// ケバブケースで記載されたExternalRefsの値を変換するためのコンバーターです．
    /// </summary>
    /// <remarks>ライブラリがSPDX公式のJSONスキーマがPACKAGE_MANAGERとPACKAGE-MANAGERで揺れていた影響を受けているため，このコンバーターによって正しい形式の文書が解析できるように対処します．</remarks>
    public class ExternalRefCategoryJsonConverter : JsonConverter<SpdxModels.ExternalRefCategory>
    {
        public override SpdxModels.ExternalRefCategory Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            var value = reader.GetString()!.Replace("-", "_");
            return (SpdxModels.ExternalRefCategory)Enum.Parse(typeof(SpdxModels.ExternalRefCategory), value);
        }

        public override void Write(Utf8JsonWriter writer, SpdxModels.ExternalRefCategory value, JsonSerializerOptions options)
        {
            var sValue = value.ToString().Replace("_", "-");
            writer.WriteStringValue(sValue);
        }
    }
}
