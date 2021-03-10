using IptablesCtl.Native.Extentions;

namespace IptablesCtl.Models.Builders.Extentions
{
    public class CommentMatchBuilder : OptionsBuilder<CommentOptions, Match>
    {
        public const string COMMENT_OPT = "--comment";
        public const string NAME = "comment";

        public CommentMatchBuilder()
        {

        }

        public CommentMatchBuilder(CommentOptions options)
        {
            SetOptions(options);
        }

        public CommentMatchBuilder(Match match) : base(match)
        {

        }


        public override Match Build()
        {
            return new Match(NAME, true, Properties);
        }

        public override CommentOptions BuildNative()
        {
            var match = Build();
            CommentOptions opt = new CommentOptions();
            if (match.TryGetOption(COMMENT_OPT, out var options))
            {
                opt.comment = options.Value;
            }
            return opt;
        }

        public override void SetOptions(CommentOptions options)
        {
            if(!string.IsNullOrEmpty(options.comment))
            {
                SetComment(options.comment);
            }
        }

        public CommentMatchBuilder SetComment(string comment)
        {
            AddProperty(COMMENT_OPT.ToOptionName(), comment);
            return this;
        }
    }
}