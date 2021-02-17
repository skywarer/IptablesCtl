using System;
using System.Linq;
using IptablesCtl.Native;
namespace IptablesCtl.Models.Builders
{
    public sealed class OwnerMatchBuilder : OptionsBuilder<OwnerOptions, Match>
    {

        public const byte Revision = 1;
        public const string UID_OWNER_OPT = "--uid-owner";
        public const string GID_OWNER_OPT = "--gid-owner";
        public const string PID_OWNER_OPT = "--pid-owner";
        public const string SOCKET_EXSTS_OPT = "--socket-exists";

        public OwnerMatchBuilder() { }
        public OwnerMatchBuilder(OwnerOptions options)
        {
            SetOptions(options);
        }
        public OwnerMatchBuilder(Match match) : base(match)
        {

        }
        public override void SetOptions(OwnerOptions options)
        {
            //uid-owner
            if ((options.match & OwnerOptions.XT_OWNER_UID) > 0)
            {
                SetUid(options.uid_min, options.uid_max, (options.invert & OwnerOptions.XT_OWNER_UID) > 0);
            }
            //gid-owner
            if ((options.match & OwnerOptions.XT_OWNER_GID) > 0)
            {
                SetGid(options.gid_min, options.gid_max, (options.invert & OwnerOptions.XT_OWNER_GID) > 0);
            }
            //socket-exists
            if ((options.match & OwnerOptions.XT_OWNER_SOCKET) > 0)
            {
                SetSocketExists((options.invert & OwnerOptions.XT_OWNER_SOCKET) > 0);
            }
        }

        public OwnerMatchBuilder SetUid(uint min, uint max, bool invert = false)
        {
            AddRangeProperty(UID_OWNER_OPT.ToOptionName(invert), min, max, '-');
            return this;
        }

        public OwnerMatchBuilder SetGid(uint min, uint max, bool invert = false)
        {
            AddRangeProperty(GID_OWNER_OPT.ToOptionName(invert), min, max, '-');
            return this;
        }

        public OwnerMatchBuilder SetSocketExists(bool invert = false)
        {
            AddProperty(SOCKET_EXSTS_OPT.ToOptionName(invert));
            return this;
        }
        public override Match Build()
        {
            return new Match(MatchTypes.OWNER, true, Properties, Revision);
        }

        public override OwnerOptions BuildNative()
        {
            var match = Build();
            OwnerOptions opt = new OwnerOptions();
            //uid-owner
            if (match.TryGetOption(UID_OWNER_OPT, out var options))
            {
                var range = options.Value.ToRangeProperty('-');
                opt.uid_min = uint.Parse(range.Left);
                opt.uid_max = uint.Parse(range.Rigt);
                opt.match |= OwnerOptions.XT_OWNER_UID;
                if (options.Inverted) opt.invert |= OwnerOptions.XT_OWNER_UID;
            }
            //gid-owner
            if (match.TryGetOption(GID_OWNER_OPT, out options))
            {
                var range = options.Value.ToRangeProperty('-');
                opt.gid_min = uint.Parse(range.Left);
                opt.gid_max = uint.Parse(range.Rigt);
                opt.match |= OwnerOptions.XT_OWNER_GID;
                if (options.Inverted) opt.invert |= OwnerOptions.XT_OWNER_GID;
            }
            //socket-exists
            if (match.TryGetOption(SOCKET_EXSTS_OPT, out options))
            {
                opt.match |= OwnerOptions.XT_OWNER_SOCKET;
                if (options.Inverted) opt.invert |= OwnerOptions.XT_OWNER_SOCKET;
            }
            return opt;
        }

    }
}