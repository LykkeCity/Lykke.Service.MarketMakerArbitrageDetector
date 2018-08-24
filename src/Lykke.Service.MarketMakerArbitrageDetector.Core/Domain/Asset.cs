using System;

namespace Lykke.Service.MarketMakerArbitrageDetector.Core.Domain
{
    public class Asset
    {
        public string Id { get; }

        public string Name { get; }


        public Asset(string id, string name)
        {
            Id = id;
            Name = name;
        }

        public bool IsValid()
        {
            return !string.IsNullOrWhiteSpace(Id) && !string.IsNullOrWhiteSpace(Name);
        }

        #region Equals and GetHashCode

        public bool Equals(Asset other)
        {
            return Id == other.Id && Name == other.Name;
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (obj == null)
                return false;

            if (!(obj is Asset other))
                throw new InvalidCastException(nameof(obj));

            return Equals(other);
        }

        /// <inheritdoc />
        public override int GetHashCode()
        {
            unchecked
            {
                return (Id != null ? Id.GetHashCode() : 0) * 397;
            }
        }

        #endregion

        public override string ToString()
        {
            return $"Id: {Id}, Name: {Name}";
        }
    }
}
