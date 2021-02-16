using System;
namespace IptablesCtl.Models
{
    public record RangeProperty<T>(T Left, T Rigt, char Delim) where T : IComparable<T>
    {
        public override string ToString()
        {            
            var order = Left.CompareTo(Rigt);
            return order < 0 ? $"{Left}{Delim}{Rigt}"
                : order > 0 ? $"{Rigt}{Delim}{Left}"
                    : $"{Left}";
        }
    }
}