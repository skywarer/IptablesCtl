using System;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Linq;
namespace IptablesCtl.Models.Serialization
{
    public class TargetConverter : JsonConverter<Target>
    {
        public override Target Read(ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            string name = "unknown target";
            IDictionary<string, string> prop = System.Collections.Immutable.ImmutableDictionary<string, string>.Empty;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new Target(name, prop);
                }
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "Name":
                            name = reader.GetString();
                            break;
                        case "Options":
                            prop = JsonSerializer.Deserialize<IDictionary<string, string>>(ref reader, options);
                            break;
                    }
                }
            }
            return new Target(name, prop);
        }

        public override void Write(
            Utf8JsonWriter writer,
            Target target,
            JsonSerializerOptions srlzOpt)
        {
            writer.WriteStartObject();
            writer.WriteString("Name", target.Name);
            if (target.Any())
            {
                writer.WritePropertyName("Options");
                JsonSerializer.Serialize<IDictionary<string, string>>(writer, target, srlzOpt);
            }
            writer.WriteEndObject();
        }
    }
}