using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace IptablesCtl.Models.Serialization
{
    public class RuleConverter : JsonConverter<Rule>
    {
        public override Rule Read(ref Utf8JsonReader reader,
            Type typeToConvert,
            JsonSerializerOptions options)
        {
            if (reader.TokenType != JsonTokenType.StartObject)
            {
                throw new JsonException();
            }
            IDictionary<string, string> prop = System.Collections.Immutable.ImmutableDictionary<string, string>.Empty;
            IList<Match> matches = System.Collections.Immutable.ImmutableList<Match>.Empty;
            Target target = null;
            while (reader.Read())
            {
                if (reader.TokenType == JsonTokenType.EndObject)
                {
                    return new Rule(prop, matches, target);
                }
                if (reader.TokenType == JsonTokenType.PropertyName)
                {
                    var propertyName = reader.GetString();
                    reader.Read();
                    switch (propertyName)
                    {
                        case "Matches":
                            matches = JsonSerializer.Deserialize<IList<Match>>(ref reader, options);
                            break;
                        case "Target":
                            target = JsonSerializer.Deserialize<Target>(ref reader, options);
                            break;
                        case "Options":
                            prop = JsonSerializer.Deserialize<IDictionary<string, string>>(ref reader, options);
                            break;
                    }
                }
            }
            return new Rule(prop, matches, target);
        }

        public override void Write(
            Utf8JsonWriter writer,
            Rule rule,
            JsonSerializerOptions srlzOpt)
        {
            writer.WriteStartObject();
            if (rule.Matches.Any())
            {
                writer.WritePropertyName("Matches");
                JsonSerializer.Serialize<IReadOnlyCollection<Match>>(writer, rule.Matches, srlzOpt);
            }
            if (rule.Target != null)
            {
                writer.WritePropertyName("Target");
                JsonSerializer.Serialize<Target>(writer, rule.Target, srlzOpt);
            }
            if (rule.Any())
            {
                writer.WritePropertyName("Options");
                JsonSerializer.Serialize<IDictionary<string, string>>(writer, rule, srlzOpt);
            }
            writer.WriteEndObject();
        }
    }
}