using System.Collections.Generic;
using NFluent;
using TrainTrain.Domain;
using Xunit;

namespace TrainTrain.Test.TDD.Unit
{
    public class ReservationShould
    {
        [Fact]
        public void Be_value_object()
        {
            const string trainId = "9043-2019-03-13";
            var reservation = new Reservation(new TrainId(trainId), new BookingReference("1BW80"),
                new List<Seat> {new Seat("A", 1)});
            var sameReservation = new Reservation(new TrainId(trainId), new BookingReference("1BW80"),
                new List<Seat> {new Seat("A", 1)});

            Check.That(sameReservation).IsEqualTo(reservation);
        }
    }
}