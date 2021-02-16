using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
namespace IptablesCtl.Models
{
    [JsonConverter(typeof(Serialization.TargetConverter))]
    public class Target : Options
    {
        public string Name { get; }

        public Target() : base(ImmutableDictionary<string, string>.Empty)
        { }

        public Target(string name) : base(ImmutableDictionary<string, string>.Empty)
        {
            Name = name;
        }

        public Target(string name, IDictionary<string, string> prop) : base(prop)
        {
            Name = name;
        }

        public override string ToString()
        {
            return $"-j {Name} {base.ToString()}".Trim();
        }
    }
}