namespace IptablesCtl.Models
{
    public record OptionValue(string Value, bool Inverted)
    {
        public override string ToString()
        {
            return Value;
        }
    }
}