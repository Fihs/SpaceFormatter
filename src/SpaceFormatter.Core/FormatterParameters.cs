using System;
using System.Collections.Generic;

namespace SpaceFormatter.Core
{
    public struct FormatterParameters : IEquatable<FormatterParameters>
    {
        #region Constructors

        public FormatterParameters(string path, bool clear, bool random, ulong size)
        {
            Path = path;
            Random = random;
            Clear = clear;
            Size = size;
        }

        #endregion Constructors

        #region Properties

        public string Path { get; }
        public bool Random { get; set; }
        public bool Clear { get; set; }
        public ulong Size { get; }

        #endregion Properties

        #region Methods

        public static bool operator ==(FormatterParameters left, FormatterParameters right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FormatterParameters left, FormatterParameters right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is FormatterParameters parameters && Equals(parameters);
        }

        public bool Equals(FormatterParameters other)
        {
            return Path == other.Path &&
                   Random == other.Random &&
                   Clear == other.Clear &&
                   Size == other.Size;
        }

        public override int GetHashCode()
        {
            int hashCode = -617146282;
            hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode(Path);
            hashCode = hashCode * -1521134295 + Random.GetHashCode();
            hashCode = hashCode * -1521134295 + Clear.GetHashCode();
            hashCode = hashCode * -1521134295 + Size.GetHashCode();
            return hashCode;
        }

        #endregion Methods
    }
}