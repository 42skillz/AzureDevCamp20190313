using System;
using System.Collections.Generic;
using Value;

namespace TrainTrain.Domain
{
    public class BookingReference : ValueType<BookingReference>
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

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new object[] {Id};
        }
    }
}