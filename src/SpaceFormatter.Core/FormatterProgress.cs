using System;

namespace SpaceFormatter.Core
{
    public struct FormatterProgress : IEquatable<FormatterProgress>
    {
        #region Constructors

        public FormatterProgress(double percentage, long count, long current, TimeSpan etc)
        {
            Percentage = percentage;
            Count = count;
            Current = current;
            Etc = etc;
        }

        #endregion Constructors

        #region Properties

        public double Percentage { get; }
        public long Count { get; }
        public long Current { get; }
        public TimeSpan Etc { get; }

        #endregion Properties

        #region Methods

        public static bool operator ==(FormatterProgress left, FormatterProgress right)
        {
            return left.Equals(right);
        }

        public static bool operator !=(FormatterProgress left, FormatterProgress right)
        {
            return !(left == right);
        }

        public override bool Equals(object obj)
        {
            return obj is FormatterProgress progress && Equals(progress);
        }

        public bool Equals(FormatterProgress other)
        {
            return Percentage == other.Percentage &&
                   Count == other.Count &&
                   Current == other.Current &&
                   Etc.Equals(other.Etc);
        }

        public override int GetHashCode()
        {
            int hashCode = -33907528;
            hashCode = hashCode * -1521134295 + Percentage.GetHashCode();
            hashCode = hashCode * -1521134295 + Count.GetHashCode();
            hashCode = hashCode * -1521134295 + Current.GetHashCode();
            hashCode = hashCode * -1521134295 + Etc.GetHashCode();
            return hashCode;
        }

        #endregion Methods
    }
}