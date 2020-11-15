using System.Collections.Generic;

namespace TrainTrain.Domain
{
    public record ReservationFailure : Reservation
    {
        public ReservationFailure(TrainId trainId) : base(trainId, new BookingReference(), new List<Seat>())
        {
        }
    }
}