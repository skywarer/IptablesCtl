using System;
namespace IptablesCtl.Models
{
    public readonly struct RangeProperty<T> where T : IComparable<T>
    {
        public readonly T Left;
        public readonly T Rigt;
        public readonly char Delim;

        public RangeProperty(T left, T right, char delim)
        {
            Left = left;
            Rigt = right;
            Delim = delim;
        }
        public override string ToString()
        {            
            var order = Left.CompareTo(Rigt);
            return order < 0 ? $"{Left}{Delim}{Rigt}"
                : order > 0 ? $"{Rigt}{Delim}{Left}"
                    : $"{Left}";
        }
    }


}