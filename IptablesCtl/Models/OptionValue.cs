namespace IptablesCtl.Models
{
    public readonly struct OptionValue
    {
        public readonly string Value;
        public readonly bool Inverted;

        public OptionValue(string value, bool inverted)
        {
            Value = value;
            Inverted = inverted;
        }
        public override string ToString()
        {
            return Value;
        }
    }
}