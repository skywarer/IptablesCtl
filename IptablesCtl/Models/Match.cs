using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Text.Json.Serialization;
using IptablesCtl.Native;
using System.Runtime.InteropServices;
using IptablesCtl.IO;

namespace IptablesCtl.Models
{
    [JsonConverter(typeof(Serialization.MatchConverter))]
    public class Match : Options
    {
        public string Name { get; }
        public bool NeedKey { get; }

        /// <summary>
        /// Version of struct
        /// </summary>
        public byte Revision {get;}

        public Match(string name, bool needKey = false, byte revision = 0) : this(name,needKey,ImmutableDictionary<string, string>.Empty,revision)
        {
            
        }

        public Match(string name, bool needKey, IDictionary<string, string> prop, byte revision = 0) : base(prop)
        {
            Name = name;
            NeedKey = needKey;
            Revision = revision;
        }

        public override string ToString()
        {
            var key = NeedKey ? $"-m {Name}" : String.Empty;
            return $"{key} {base.ToString()}".Trim();
        }

    }

    public abstract class Match<T> : Match where T : struct
    {
        public Match(string name, bool needKey) : base(name, needKey)
        {
        }

        public abstract T BuildOptions();

        public int WriteOptions(IntPtr point)
        {
            Header header = new Header();
            header.name = Name;
            var size = HeaderLen + Marshal.SizeOf<T>();
            if(size >= ushort.MaxValue) throw new IO.IptException($"size of {nameof(T)} greater than [{ushort.MaxValue}]");
            header.size = (ushort)Sizes.Align(size);
            Marshal.StructureToPtr(header, point, true);
            var options = BuildOptions();
            Marshal.StructureToPtr(options, point + HeaderLen, true);
            return header.size;
        }
    }
}
