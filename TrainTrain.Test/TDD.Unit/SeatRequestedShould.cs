using NFluent;
using System;
using TrainTrain.Domain;
using Xunit;

namespace TrainTrain.Test.TDD.Unit
{
    public class SeatRequestedShould
    {
        [Fact]
        public void Be_value_object()
        {
            var seatsRequested = new SeatsRequested(3);
            var sameSeatsRequested = new SeatsRequested(3);

            Check.That(seatsRequested).IsEqualTo(sameSeatsRequested);
        }

        [Fact]
        public void Raise_exception_when_seats_requested_count_is_invalid()
        {
            Check.ThatCode(() => new SeatsRequested(SeatsRequested.MinRequested - 1)).Throws<ArgumentException>()
                .WithMessage($"seatRequestCount({SeatsRequested.MinRequested - 1}) should be between {SeatsRequested.MinRequested} and {SeatsRequested.MaxRequested}");

            Check.ThatCode(() => new SeatsRequested(SeatsRequested.MaxRequested + 1)).Throws<ArgumentException>()
                .WithMessage($"seatRequestCount({SeatsRequested.MaxRequested + 1}) should be between {SeatsRequested.MinRequested} and {SeatsRequested.MaxRequested}");
        }
    }
}