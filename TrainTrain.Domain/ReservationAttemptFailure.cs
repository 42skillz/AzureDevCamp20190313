using System.Collections.Generic;

namespace TrainTrain.Domain
{
    public record ReservationAttemptFailure : ReservationAttempt
    {
        public ReservationAttemptFailure(TrainId trainId, SeatsRequested seatsRequested) : base(trainId, seatsRequested,
            new List<Seat>())
        {
        }
    }
}