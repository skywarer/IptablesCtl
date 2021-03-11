#define DEBUG
using IptablesCtl.Native;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
namespace IptablesCtl.Models
{
    [JsonConverter(typeof(Serialization.RuleConverter))]
    public class Rule : Options
    {
        public IReadOnlyCollection<Match> Matches { get; }
        public Target Target { get; }

        [JsonIgnore]
        public ulong Bytes {get;}
        [JsonIgnore]
        public ulong Packages {get;}

        public Rule() : base(ImmutableDictionary<string, string>.Empty)
        {

        }

        public Rule(IDictionary<string, string> prop, IList<Match> matches, Target target) : base(prop)
        {
            Matches = new ReadOnlyCollection<Match>(matches);
            Target = target;
        }

        public Rule(IDictionary<string, string> prop, IList<Match> matches, Target target, ulong bts, ulong pkgs) :
            this(prop,matches,target)
        {
           Bytes = bts;
           Packages = pkgs;
        }

        public override string ToString()
        {
            string[] lines = { base.ToString(), String.Join(' ', Matches), Target.ToString() };
            return String.Join(' ', lines.Where(l => !String.IsNullOrEmpty(l)));
        }
    }
}