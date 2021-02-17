using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
namespace IptablesCtl.Models
{
    [JsonConverter(typeof(Serialization.TargetConverter))]
    public class Target : Options
    {
        public string Name { get; }

        /// <summary>
        /// Version of struct
        /// </summary>
        public byte Revision { get; }

        public Target(byte revision = 0) : this(string.Empty, ImmutableDictionary<string, string>.Empty, revision)
        {
        }

        public Target(string name, byte revision = 0) : this(name, ImmutableDictionary<string, string>.Empty, revision)
        {
        }

        public Target(string name, IDictionary<string, string> prop, byte revision = 0) : base(prop)
        {
            Name = name;
            Revision = revision;
        }

        public override string ToString()
        {
            return $"-j {Name} {base.ToString()}".Trim();
        }
    }
}