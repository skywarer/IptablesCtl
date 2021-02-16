namespace IptablesCtl.Models
{
    public readonly struct OptionName
    {

        public readonly string Name;
        public readonly bool Inverted;
        public OptionName(string name, bool inverted)
        {
            Name = name;
            Inverted = inverted;
        }
        public override string ToString()
        {
            return Inverted ? $"! {Name}" : Name;
        }
    }
}