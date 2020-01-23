using System.Collections.Generic;
using System.Linq;
using Value;

namespace TrainTrain.Domain
{
    public class Reservation : ValueType<Reservation>
    {
        private readonly List<Seat> _seats;
        public TrainId TrainId { get; }
        public BookingReference BookingReference { get; }
        public IReadOnlyCollection<Seat> Seats => _seats;

        public Reservation(ReservationAttempt reservationAttempt)
            : this(reservationAttempt.TrainId, reservationAttempt.BookingReference, reservationAttempt.Seats)
        {
        }

        public Reservation(TrainId trainId, BookingReference bookingReference, IEnumerable<Seat> seats)
        {
            TrainId = trainId;
            BookingReference = bookingReference;
            _seats = seats.ToList();
        }

        public Reservation(TrainId trainId) : this(trainId, new BookingReference(string.Empty), new List<Seat>())
        {
        }

        protected override IEnumerable<object> GetAllAttributesToBeUsedForEquality()
        {
            return new object[] {TrainId, BookingReference, new ListByValue<Seat>(_seats)};
        }
    }
}