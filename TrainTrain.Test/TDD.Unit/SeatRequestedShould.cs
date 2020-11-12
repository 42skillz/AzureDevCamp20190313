using System;
using NFluent;
using NUnit.Framework;
using TrainTrain.Domain;

namespace TrainTrain.Test.TDD.Unit
{
    internal class SeatRequestedShould
    {
        [Test]
        public void Be_value_object()
        {
            var seatsRequested = new SeatsRequested(3);
            var sameSeatsRequested = new SeatsRequested(3);

            Check.That(seatsRequested).IsEqualTo(sameSeatsRequested);
        }

        [Test]
        public void Raise_exception_when_seats_requested_count_is_invalid()
        {
            Check.ThatCode(() => new SeatsRequested(SeatsRequested.MinRequested -1)).Throws<ArgumentException>()
                .WithMessage($"seatRequestCount({SeatsRequested.MinRequested - 1}) should be between 1 and 20");
            Check.ThatCode(() => new SeatsRequested(SeatsRequested.MaxRequested +1)).Throws<ArgumentException>()
                .WithMessage($"seatRequestCount({SeatsRequested.MaxRequested + 1}) should be between 1 and 20");
        }
    }
}