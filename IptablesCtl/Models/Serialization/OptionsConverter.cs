using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IptablesCtl.Models.Serialization
{
    public class OptionsConverter : JsonConverter<Options>
    {
        public override Options Read(ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            var optDict = JsonSerializer.Deserialize<IDictionary<string, string>>(ref reader, options);
            return new Options(optDict);
        }

        public override void Write(
            Utf8JsonWriter writer,
            Options options,
            JsonSerializerOptions srlzOpt)
        {
            JsonSerializer.Serialize<IDictionary<string, string>>(writer, options, srlzOpt);
        }
    }
}