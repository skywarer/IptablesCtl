namespace IptablesCtl.Models
{
    public record OptionName(string Name, bool Inverted)
    {
        public override string ToString()
        {
            return Inverted ? $"! {Name}" : Name;
        }
    }
}