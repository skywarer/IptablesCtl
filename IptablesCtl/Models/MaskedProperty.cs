using System;
namespace IptablesCtl.Models
{
    public readonly struct MaskedProperty
    {
        public readonly string Value;
        public readonly string Mask;
        public readonly char Delim;

        public MaskedProperty(string value,string mask,char delim)
        {
            Value = value;
            Mask = mask;
            Delim = delim;
        }

        public override string ToString()
        {
            return string.IsNullOrEmpty(Mask) ? Value : $"{Value}{Delim}{Mask}";
        }
    }

    public readonly struct MaskedProperty<T> where T : IComparable<T>
    {
        public readonly T Value;
        public readonly T Mask;
        public readonly char Delim;

        public MaskedProperty(T value,T mask,char delim)
        {
            Value = value;
            Mask = mask;
            Delim = delim;
        }

        public override string ToString()
        {
            return Mask.CompareTo(default(T)) == 0 ? $"{Value}" : $"{Value}{Delim}{Mask}";
        }
    }
}