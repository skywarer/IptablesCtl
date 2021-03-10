using System;
using System.Collections.Generic;
using IptablesCtl.Native.Extentions;
using IptablesCtl.Models.Builders.Extentions;
using IptablesCtl.Models;
using IptablesCtl.Native;

namespace IptablesCtl.IO.Extentions
{
    public class IptExtended : IptTransaction
    {
        public IptExtended(string tableName = Tables.FILTER) : base(tableName)
        {

        }

        protected override Type GetMatchOptionsType(string name) => name.ToLower() switch
        {
            CommentMatchBuilder.NAME => typeof(CommentOptions),
            _ => null
        };

        protected override IDictionary<string, string> GetMatchOptions(Header header, object options) => header.name.ToLower() switch
        {           
            CommentMatchBuilder.NAME => new CommentMatchBuilder((CommentOptions)options).Build(),
            _ => null
        };

        protected override object SetMatchOptions(Match match) => match.Name switch
        {
            CommentMatchBuilder.NAME => new CommentMatchBuilder(match).BuildNative(),
            _ => null
        };
    }
}