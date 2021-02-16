using System;
namespace IptablesCtl.Models
{
    public record MaskedProperty(string Value, string Mask, char Delim)
    {
        public override string ToString()
        {
            return string.IsNullOrEmpty(Mask) ? Value : $"{Value}{Delim}{Mask}";
        }
    }

    public record MaskedProperty<T>(T Value, T Mask, char Delim) where T : IComparable<T>
    {
        public override string ToString()
        {
            return Mask.CompareTo(default(T)) == 0 ? $"{Value}" : $"{Value}{Delim}{Mask}";
        }
    }
}