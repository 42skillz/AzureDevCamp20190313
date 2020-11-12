using System;

namespace TrainTrain.Domain
{
    public sealed record BookingReference
    {
        public const int MaxLength = 7;
        public string Id { get; }

        public BookingReference() : this(string.Empty)
        {
        }

        public BookingReference(string id)
        {
            if (!string.IsNullOrEmpty(id) && id.Length > MaxLength)
                throw new ArgumentException($"{nameof(id)} length should less than {MaxLength}");

            Id = id;
        }

        public override string ToString()
        {
            return Id;
        }

        public bool IsValid()
        {
            return !string.IsNullOrEmpty(Id);
        }
    }
}