using System;
using System.Collections.Generic;
using System.Linq;
using Value;
using Value.Shared;

namespace TrainTrain.Domain
{
    public class Train : ValueType<Train>
    {
        private readonly Dictionary<string, Coach> _coaches;
        private TrainId TrainId { get; }

        public IReadOnlyDictionary<string, Coach> Coaches => _coaches;

        private int NumberOfReservedSeats
        {
            get { return Seats.Count(s => !s.IsAvailable()); }
        }

        private List<Seat> Seats
        {
            get { return Coaches.Values.SelectMany(c => c.Seats).ToList(); }
        }


        public Train(TrainId trainId, Dictionary<string, Coach> coaches)
        {
            TrainId = trainId;
            _coaches = coaches;
        }

        public bool DoesNotExceedOverallCapacity(SeatsRequested seatsRequested)
        {
            return NumberOfReservedSeats + seatsRequested.Count <=
                   Math.Floor(CapacityThresholdPolicy.ForTrain * Seats.Count);
        }

        public ReservationAttempt BuildReservationAttempt(SeatsRequested seatsRequested)
        {
            var attemptInTheSameCoach = BuildReservationAttemptInTheSameCoach(seatsRequested);

            if (attemptInTheSameCoach.IsFulFilled) return attemptInTheSameCoach;

            return BuildReservationAttemptForOverallTrainCapacity(seatsRequested);
        }

        private ReservationAttempt BuildReservationAttemptForOverallTrainCapacity(SeatsRequested seatsRequested)
        {
            return new ReservationAttempt(TrainId, seatsRequested, Seats
                .Where(s => s.IsAvailable()).Take(seatsRequested.Count));
        }

        private ReservationAttempt BuildReservationAttemptInTheSameCoach(SeatsRequested seatsRequested)
        {
            foreach (var coach in Coaches.Values)
                if (coach.DoesNotExceedOverallCapacity(seatsRequested))
                {
                    var reservationAttempt = coach.BuildReservationAttempt(TrainId, seatsRequested);

                    if (reservationAttempt.IsFulFilled) return reservationAttempt;
                }

            return new ReservationAttemptFailure(TrainId, seatsRequested);
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new object[] {TrainId, new DictionaryByValue<string, Coach>(_coaches)};
        }
    }
}