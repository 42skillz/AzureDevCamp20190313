using System.Collections.Generic;
using NFluent;
using TrainTrain.Domain;
using Xunit;

namespace TrainTrain.Test.TDD.Unit
{
    public class ReservationAttemptShould
    {
        [Fact]
        public void Be_value_object()
        {
            const string trainId = "9043-2019-03-13";

            var reservationAttempt = new ReservationAttempt(new TrainId(trainId), new SeatsRequested(3),
                new List<Seat> {new Seat("A", 1)});
            var sameReservationAttempt = new ReservationAttempt(new TrainId(trainId), new SeatsRequested(3),
                new List<Seat> {new Seat("A", 1)});

            Check.That(reservationAttempt).IsEqualTo(sameReservationAttempt);
        }
    }
}