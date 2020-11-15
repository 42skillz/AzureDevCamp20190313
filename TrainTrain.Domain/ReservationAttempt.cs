using System.Collections.Generic;
using System.Linq;
using CollectionByValue;

namespace TrainTrain.Domain
{
    public record ReservationAttempt
    {
        private readonly ListValue<Seat> _seats;
        public TrainId TrainId { get; }
        public IEnumerable<Seat> Seats => _seats.Item;
        public BookingReference BookingReference { get; }
        private SeatsRequested SeatsRequested { get; }

        public bool IsFulFilled => Seats.Count() == SeatsRequested.Count;

        public ReservationAttempt(TrainId trainId, SeatsRequested seatsRequestedCount, IEnumerable<Seat> seats)
            : this(trainId, new BookingReference(), seatsRequestedCount, seats)
        {
        }

        private ReservationAttempt(TrainId trainId, BookingReference bookingReference,
            SeatsRequested seatsRequestedCount, IEnumerable<Seat> seats)
        {
            TrainId = trainId;
            BookingReference = bookingReference;
            SeatsRequested = seatsRequestedCount;
            _seats = new ListValue<Seat>(new List<Seat>(seats));
        }

        // DDD Pattern: Closure Of Operation
        public ReservationAttempt AssignBookingReference(BookingReference bookingReference)
        {
            var assignedSeats = Seats.Select(seat => new Seat(seat.CoachName, seat.SeatNumber, BookingReference));
            return new ReservationAttempt(TrainId, bookingReference, SeatsRequested, assignedSeats);
        }
    }
}