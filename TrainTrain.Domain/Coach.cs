using System;
using System.Collections.Generic;
using System.Linq;
using CollectionByValue;

namespace TrainTrain.Domain
{
    public sealed record Coach 
    {
        private readonly ListValue<Seat> _seats;
        public IEnumerable<Seat> Seats => _seats.Item;
        public string Name { get; }

        private int NumberOfReservedSeats
        {
            get { return Seats.Count(s => !s.IsAvailable()); }
        }

        public Coach(string name) : this(name, new List<Seat>())
        {
        }

        public Coach(string name, IList<Seat> seats)
        {
            Name = name;
            _seats = new ListValue<Seat>(seats);
        }

        // DDD Pattern: Closure Of Operation
        public Coach AddSeat(Seat seat)
        {
            return new Coach(seat.CoachName, new List<Seat>(Seats) {seat});
        }

        public ReservationAttempt BuildReservationAttempt(TrainId trainId, SeatsRequested seatsRequested)
        {
            var availableSeats = Seats.Where(s => s.IsAvailable()).Take(seatsRequested.Count).ToList();
            return seatsRequested.IsMatch(availableSeats)
                ? new ReservationAttempt(trainId, seatsRequested, availableSeats)
                : new ReservationAttemptFailure(trainId, seatsRequested);
        }

        public bool DoesNotExceedOverallCapacity(SeatsRequested seatsRequested)
        {
            return NumberOfReservedSeats + seatsRequested.Count <=
                   Math.Floor(CapacityThresholdPolicy.ForCoach * Seats.Count());
        }
    }
}