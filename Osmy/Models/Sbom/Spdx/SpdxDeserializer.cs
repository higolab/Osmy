﻿using SpdxModels = CycloneDX.Spdx.Models.v2_2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;

namespace Osmy.Models.Sbom.Spdx
{
    internal class SpdxDeserializer
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

            return JsonSerializer.Deserialize<SpdxModels.SpdxDocument>(stream, options) ?? throw new FileFormatException();
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
