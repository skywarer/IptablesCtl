using System;
using System.Linq;
using System.Collections.Generic;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IptablesCtl.Models.Serialization
{
    public class MatchConverter : JsonConverter<Match>
    {
        public override Match Read(ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            string name = "unknown match";
            bool needKey = false;
            IDictionary<string, string> prop = System.Collections.Immutable.ImmutableDictionary<string, string>.Empty;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new Match(name, needKey, prop);
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
                        case "NeedKey":
                            needKey = reader.GetBoolean();
                            break;
                        case "Options":
                            prop = JsonSerializer.Deserialize<IDictionary<string, string>>(ref reader, options);
                            break;
                    }
                }
            }
            return new Match(name, needKey, prop);
        }

        public override void Write(
            Utf8JsonWriter writer,
            Match match,
            JsonSerializerOptions srlzOpt)
        {
            writer.WriteStartObject();
            writer.WriteString("Name", match.Name);
            writer.WriteBoolean("NeedKey", match.NeedKey);
            if (match.Any())
            {
                writer.WritePropertyName("Options");
                JsonSerializer.Serialize<IDictionary<string, string>>(writer, match, srlzOpt);
            }
            writer.WriteEndObject();
        }
    }
}